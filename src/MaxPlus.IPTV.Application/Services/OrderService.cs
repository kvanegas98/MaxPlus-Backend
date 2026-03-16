using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository            _repository;
    private readonly IIptvAccountRepository      _iptvAccountRepository;
    private readonly ICustomerRepository         _customerRepository;
    private readonly IServiceTypeRepository      _serviceTypeRepository;
    private readonly IInvoiceRepository          _invoiceRepository;
    private readonly IPlataformaConfigRepository _plataformaConfigRepo;
    private readonly IWhatsAppService            _whatsApp;
    private readonly IEmailService               _email;
    private readonly IPdfTicketService           _pdfTicket;
    private readonly IStorageService             _storage;

    public OrderService(
        IOrderRepository            repository,
        IIptvAccountRepository      iptvAccountRepository,
        ICustomerRepository         customerRepository,
        IServiceTypeRepository      serviceTypeRepository,
        IInvoiceRepository          invoiceRepository,
        IPlataformaConfigRepository plataformaConfigRepo,
        IWhatsAppService            whatsApp,
        IEmailService               email,
        IPdfTicketService           pdfTicket,
        IStorageService             storage)
    {
        _repository            = repository;
        _iptvAccountRepository = iptvAccountRepository;
        _customerRepository    = customerRepository;
        _serviceTypeRepository = serviceTypeRepository;
        _invoiceRepository     = invoiceRepository;
        _plataformaConfigRepo  = plataformaConfigRepo;
        _whatsApp              = whatsApp;
        _email                 = email;
        _pdfTicket             = pdfTicket;
        _storage               = storage;
    }

    public async Task<ServiceOrderResponseDto> CreateAsync(ServiceOrderCreateDto dto, string? ipAddress)
    {
        var order = new ServiceOrder
        {
            CustomerName   = dto.CustomerName.Trim(),
            CustomerPhone  = dto.CustomerPhone?.Trim(),
            CustomerEmail  = dto.CustomerEmail?.Trim(),
            TipoServicioId = dto.TipoServicioId,
            Notes          = dto.Notes?.Trim(),
            IpAddress      = ipAddress
        };

        (order.Id, order.NumeroOrden) = await _repository.AddAsync(order);

        return MapToDto(order);
    }

    public async Task<ServiceOrderResponseDto?> GetByIdAsync(Guid id)
    {
        var order = await _repository.GetByIdAsync(id);
        return order is null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<ServiceOrderResponseDto>> GetAllAsync(string? status = null)
    {
        var orders = await _repository.GetAllAsync(status);
        return orders.Select(MapToDto);
    }

    public async Task<ServiceOrderResponseDto> ApproveAsync(Guid id, Guid approvedBy, ServiceOrderApproveDto dto)
    {
        var order = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Orden con ID {id} no encontrada.");

        if (order.Status != "Pending")
            throw new InvalidOperationException("Solo se pueden aprobar órdenes en estado Pending.");

        // 1. Verificar que la cuenta IPTV existe
        var account = await _iptvAccountRepository.GetByIdAsync(dto.IptvAccountId)
            ?? throw new KeyNotFoundException($"Cuenta IPTV {dto.IptvAccountId} no encontrada.");

        // 2. Resolver cliente: explícito → buscar por contacto → crear nuevo
        Guid customerId;
        if (dto.CustomerId.HasValue && dto.CustomerId.Value != Guid.Empty)
        {
            customerId = dto.CustomerId.Value;
        }
        else
        {
            var existing = await _customerRepository.FindByContactAsync(
                order.CustomerEmail, order.CustomerPhone);

            customerId = existing?.Id ?? await _customerRepository.AddAsync(new Customer
            {
                Name  = order.CustomerName,
                Phone = order.CustomerPhone,
                Email = order.CustomerEmail
            });
        }

        // 3. Resolver precio, duración y plataforma desde TipoServicio de la orden
        decimal precio           = 0;
        string  conceptoServicio = "Servicio IPTV";
        string  plataforma       = "IPTV";
        var     expiration       = dto.ExpirationDate ?? DateTime.UtcNow.AddDays(30);

        if (order.TipoServicioId.HasValue)
        {
            var serviceType = await _serviceTypeRepository.GetByIdAsync(order.TipoServicioId.Value);
            if (serviceType != null)
            {
                precio           = serviceType.Price;
                conceptoServicio = serviceType.Name;
                plataforma       = serviceType.Plataforma;
                if (dto.ExpirationDate is null)
                    expiration = DateTime.UtcNow.AddDays(serviceType.DurationDays);
            }
        }

        // 4. Asignar slot en la cuenta IPTV (el SP valida disponibilidad)
        var subscriptionId = await _iptvAccountRepository.AssignClientAsync(
            dto.IptvAccountId, customerId, expiration, order.TipoServicioId,
            dto.ProfileUser?.Trim(), dto.ProfilePin?.Trim());

        // 5. Marcar la orden como aprobada
        await _repository.ApproveAsync(id, approvedBy, subscriptionId);

        // 6. Generar factura automáticamente
        var totalConDescuento = precio - dto.DiscountAmount;
        var invoice = new Invoice
        {
            CustomerName     = order.CustomerName,
            CustomerId       = customerId,
            UserId           = approvedBy,
            OrderType        = "Venta",
            PaymentMethod    = dto.PaymentMethod ?? "Orden Digital",
            MetodoPagoId     = dto.MetodoPagoId,
            PaymentReference = dto.PaymentReference,
            AmountReceived   = dto.AmountReceived ?? totalConDescuento,
            DiscountAmount   = dto.DiscountAmount,
            TotalAmount      = totalConDescuento,
            SubscriptionId   = subscriptionId,
            ServiceOrderId   = id,
            Nota             = $"Orden web: {order.NumeroOrden}",
            Details          =
            [
                new InvoiceDetail
                {
                    TipoServicioId = order.TipoServicioId,
                    Concept        = conceptoServicio,
                    Quantity       = 1,
                    UnitPrice      = precio,
                    DiscountAmount = 0,
                    SubTotal       = precio
                }
            ]
        };

        var invoiceId = await _invoiceRepository.CreateInvoiceAsync(invoice);

        // 7. Enviar credenciales + factura en un solo mensaje WhatsApp
        if (!string.IsNullOrWhiteSpace(order.CustomerPhone))
        {
            var configs      = await _plataformaConfigRepo.GetAllAsync();
            var config       = configs.FirstOrDefault(c => c.Plataforma == plataforma);
            var labelUsuario = config?.LabelUsuario ?? "Usuario";
            var tienePin     = config?.TienePin     ?? false;

            // Generar imagen PNG de la factura y subirla a Cloudinary
            string? invoiceImageUrl = null;
            try
            {
                var imgResult = await _pdfTicket.GenerateTicketImageAsync(invoiceId);
                using var stream = new MemoryStream(imgResult.Content);
                invoiceImageUrl = await _storage.UploadFileAsync(
                    stream,
                    $"factura-{imgResult.OrderNumber}.png",
                    "facturas");
            }
            catch
            {
                // La imagen de factura no debe bloquear la aprobación
            }

            await _whatsApp.SendCredentialsAsync(
                order.CustomerPhone,
                order.CustomerName,
                account.AccessUser,
                account.AccessPassword,
                account.PinCode,
                expiration,
                conceptoServicio,
                labelUsuario,
                tienePin,
                dto.ProfileUser?.Trim(),
                dto.ProfilePin?.Trim(),
                invoiceImageUrl);
        }

        // Email de respaldo — buscar email del cliente
        var customerEmail = order.CustomerEmail;
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            customerEmail = customer?.Email;
        }
        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            try
            {
                var configs      = await _plataformaConfigRepo.GetAllAsync();
                var config       = configs.FirstOrDefault(c => c.Plataforma == plataforma);
                var labelUsuario = config?.LabelUsuario ?? "Usuario";

                await _email.SendCredentialsAsync(
                    customerEmail, order.CustomerName,
                    account.AccessUser, account.AccessPassword,
                    account.PinCode, expiration,
                    conceptoServicio, labelUsuario,
                    account.PlatformUrl,
                    dto.ProfileUser?.Trim(),
                    dto.ProfilePin?.Trim());
            }
            catch { /* El email no debe bloquear la aprobación */ }
        }

        var updated = await _repository.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task RejectAsync(Guid id, Guid approvedBy, ServiceOrderRejectDto dto)
    {
        var order = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Orden con ID {id} no encontrada.");

        if (order.Status != "Pending")
            throw new InvalidOperationException("Solo se pueden rechazar órdenes en estado Pending.");

        await _repository.RejectAsync(id, approvedBy, dto.Reason);
    }

    private static ServiceOrderResponseDto MapToDto(ServiceOrder o) => new()
    {
        Id              = o.Id,
        NumeroOrden     = o.NumeroOrden,
        CustomerName    = o.CustomerName,
        CustomerPhone   = o.CustomerPhone,
        CustomerEmail   = o.CustomerEmail,
        TipoServicioId  = o.TipoServicioId,
        ServiceName     = o.ServiceName,
        Notes           = o.Notes,
        Status          = o.Status,
        ApprovedBy      = o.ApprovedBy,
        ApprovedAt      = o.ApprovedAt,
        SubscriptionId  = o.SubscriptionId,
        RejectionReason = o.RejectionReason,
        CreatedAt       = o.CreatedAt
    };
}
