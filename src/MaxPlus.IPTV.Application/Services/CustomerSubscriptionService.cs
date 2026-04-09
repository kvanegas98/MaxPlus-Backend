using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using System.IO;

namespace MaxPlus.IPTV.Application.Services;

public class CustomerSubscriptionService : ICustomerSubscriptionService
{
    private readonly ICustomerSubscriptionRepository _repository;
    private readonly ICustomerRepository             _customerRepository;
    private readonly IInvoiceRepository              _invoiceRepository;
    private readonly IServiceTypeRepository          _serviceTypeRepository;
    private readonly IWhatsAppService                _whatsApp;
    private readonly IEmailService                   _email;
    private readonly IPlataformaConfigRepository     _plataformaConfigRepo;
    private readonly IPdfTicketService               _pdfTicket;
    private readonly IStorageService                 _storage;
    private readonly IIptvAccountRepository          _iptvAccountRepository;

    public CustomerSubscriptionService(
        ICustomerSubscriptionRepository repository,
        ICustomerRepository             customerRepository,
        IInvoiceRepository              invoiceRepository,
        IServiceTypeRepository          serviceTypeRepository,
        IWhatsAppService                whatsApp,
        IEmailService                   email,
        IPlataformaConfigRepository     plataformaConfigRepo,
        IPdfTicketService               pdfTicket,
        IStorageService                 storage,
        IIptvAccountRepository          iptvAccountRepository)
    {
        _repository            = repository;
        _customerRepository    = customerRepository;
        _invoiceRepository     = invoiceRepository;
        _serviceTypeRepository = serviceTypeRepository;
        _whatsApp              = whatsApp;
        _email                 = email;
        _plataformaConfigRepo  = plataformaConfigRepo;
        _pdfTicket             = pdfTicket;
        _storage               = storage;
        _iptvAccountRepository = iptvAccountRepository;
    }

    public async Task<IEnumerable<CustomerSubscriptionResponseDto>> GetByCustomerIdAsync(Guid customerId)
    {
        var subs = await _repository.GetByCustomerIdAsync(customerId);
        return subs.Select(MapToDto);
    }

    public async Task<IEnumerable<CustomerSubscriptionResponseDto>> GetActiveAsync()
    {
        var subs = await _repository.GetActiveAsync();
        return subs.Select(MapToDto);
    }

    public async Task<IEnumerable<CustomerSubscriptionResponseDto>> GetUnassignedAsync(Guid? tipoServicioId = null)
    {
        var subs = await _repository.GetUnassignedAsync(tipoServicioId);
        return subs.Select(MapToDto);
    }

    public async Task<CustomerSubscriptionResponseDto> CreateAsync(CustomerSubscriptionCreateDto dto, Guid userId)
    {
        // Resolver cliente si se proporcionaron datos (todos opcionales)
        Guid? customerId   = null;
        string? resolvedName = null;

        bool hasCustomerData = dto.CustomerId.HasValue
            || !string.IsNullOrWhiteSpace(dto.CustomerName)
            || !string.IsNullOrWhiteSpace(dto.CustomerEmail)
            || !string.IsNullOrWhiteSpace(dto.CustomerPhone);

        if (hasCustomerData)
        {
            if (dto.CustomerId.HasValue && dto.CustomerId.Value != Guid.Empty)
            {
                customerId   = dto.CustomerId.Value;
                resolvedName = dto.CustomerName;
            }
            else
            {
                var existing = await _customerRepository.FindByContactAsync(
                    dto.CustomerEmail, dto.CustomerPhone);

                if (existing != null)
                {
                    customerId   = existing.Id;
                    resolvedName = existing.Name;
                }
                else if (!string.IsNullOrWhiteSpace(dto.CustomerName))
                {
                    customerId = await _customerRepository.AddAsync(new Customer
                    {
                        Name  = dto.CustomerName.Trim(),
                        Phone = dto.CustomerPhone?.Trim(),
                        Email = dto.CustomerEmail?.Trim()
                    });
                    resolvedName = dto.CustomerName.Trim();
                }
            }
        }

        // Calcular ExpirationDate si no se envió
        DateTime expiration = dto.ExpirationDate ?? DateTime.UtcNow.AddDays(30);
        if (dto.TipoServicioId.HasValue && dto.ExpirationDate is null)
        {
            var serviceType = await _serviceTypeRepository.GetByIdAsync(dto.TipoServicioId.Value);
            if (serviceType != null)
                expiration = DateTime.UtcNow.AddDays(serviceType.DurationDays);
        }

        var sub = new CustomerSubscription
        {
            CustomerId       = customerId,
            TipoServicioId   = dto.TipoServicioId,
            SubscriptionType = dto.SubscriptionType,
            PlatformUrl      = dto.PlatformUrl?.Trim(),
            AccessUser       = dto.AccessUser?.Trim(),
            AccessPassword   = dto.AccessPassword?.Trim(),
            PinCode          = dto.PinCode?.Trim(),
            ExpirationDate   = expiration
        };

        sub.Id = await _repository.AddAsync(sub);

        // Generar factura si hay cliente y método de pago
        Guid? invoiceId = null;
        if (customerId.HasValue && (!string.IsNullOrWhiteSpace(dto.PaymentMethod) || dto.MetodoPagoId.HasValue))
            invoiceId = await GenerateInvoiceAsync(userId, sub.Id, customerId.Value, resolvedName,
                dto.TipoServicioId, dto.PaymentMethod, dto.MetodoPagoId,
                dto.PaymentReference, dto.AmountReceived, dto.DiscountAmount);

        var result = MapToDto(sub);
        result.CustomerName = resolvedName;
        result.InvoiceId    = invoiceId;
        return result;
    }

    public async Task<CustomerSubscriptionResponseDto> AssignCustomerAsync(Guid subscriptionId, AssignCustomerDto dto, Guid userId)
    {
        var sub = await _repository.GetByIdAsync(subscriptionId)
            ?? throw new KeyNotFoundException($"Suscripcion {subscriptionId} no encontrada.");

        if (sub.CustomerId.HasValue)
            throw new InvalidOperationException("La suscripcion ya tiene un cliente asignado.");

        // Resolver cliente
        Guid customerId;
        string? resolvedName;

        if (dto.CustomerId.HasValue && dto.CustomerId.Value != Guid.Empty)
        {
            customerId   = dto.CustomerId.Value;
            resolvedName = dto.CustomerName;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                throw new InvalidOperationException("Debe enviar CustomerId o CustomerName para asignar el cliente.");

            var existing = await _customerRepository.FindByContactAsync(dto.CustomerEmail, dto.CustomerPhone);

            if (existing != null)
            {
                customerId   = existing.Id;
                resolvedName = existing.Name;
            }
            else
            {
                customerId = await _customerRepository.AddAsync(new Customer
                {
                    Name  = dto.CustomerName.Trim(),
                    Phone = dto.CustomerPhone?.Trim(),
                    Email = dto.CustomerEmail?.Trim()
                });
                resolvedName = dto.CustomerName.Trim();
            }
        }

        await _repository.AssignCustomerAsync(subscriptionId, customerId);

        // Generar factura si se envio metodo de pago
        Guid? invoiceId = null;
        if (!string.IsNullOrWhiteSpace(dto.PaymentMethod) || dto.MetodoPagoId.HasValue)
            invoiceId = await GenerateInvoiceAsync(userId, subscriptionId, customerId, resolvedName,
                sub.TipoServicioId, dto.PaymentMethod, dto.MetodoPagoId,
                dto.PaymentReference, dto.AmountReceived, dto.DiscountAmount);

        sub.CustomerId   = customerId;
        sub.CustomerName = resolvedName;
        sub.Status       = "Active";

        var result = MapToDto(sub);
        result.InvoiceId = invoiceId;
        return result;
    }

    public async Task<CustomerSubscriptionResponseDto> UpdateAsync(Guid id, CustomerSubscriptionUpdateDto dto)
    {
        var sub = new CustomerSubscription
        {
            Id             = id,
            PlatformUrl    = dto.PlatformUrl?.Trim(),
            AccessUser     = dto.AccessUser?.Trim(),
            AccessPassword = dto.AccessPassword?.Trim(),
            PinCode        = dto.PinCode?.Trim(),
            ExpirationDate = dto.ExpirationDate,
            Status         = dto.Status
        };

        await _repository.UpdateAsync(sub);
        return MapToDto(sub);
    }

    public async Task CancelAsync(Guid id)
    {
        await _repository.CancelAsync(id);
    }

    public async Task<RenewalResponseDto> RenewAsync(Guid subscriptionId, RenewalCreateDto dto, Guid userId)
    {
        var original = await _repository.GetByIdAsync(subscriptionId)
            ?? throw new KeyNotFoundException($"Suscripcion {subscriptionId} no encontrada.");

        if (!original.CustomerId.HasValue)
            throw new InvalidOperationException("No se puede renovar una suscripcion sin cliente asignado.");

        var newSubId = await _repository.RenewAsync(subscriptionId, dto.NewExpiration);

        Guid? invoiceId = null;
        if (!string.IsNullOrWhiteSpace(dto.PaymentMethod) || dto.MetodoPagoId.HasValue)
            invoiceId = await GenerateInvoiceAsync(userId, newSubId, original.CustomerId.Value, original.CustomerName,
                original.TipoServicioId, dto.PaymentMethod, dto.MetodoPagoId,
                dto.PaymentReference, dto.AmountReceived, dto.DiscountAmount,
                isRenewal: true);

        // Notificar al cliente por WhatsApp y email
        var customer = await _customerRepository.GetByIdAsync(original.CustomerId.Value);
        if (customer is not null)
        {
            // Si la suscripción no tiene credenciales propias, tomarlas de la cuenta IPTV padre
            string accessUser     = original.AccessUser     ?? string.Empty;
            string accessPassword = original.AccessPassword ?? string.Empty;
            if (string.IsNullOrEmpty(accessUser) && original.IptvAccountId.HasValue)
            {
                var account = await _iptvAccountRepository.GetByIdAsync(original.IptvAccountId.Value);
                if (account is not null)
                {
                    accessUser     = account.AccessUser;
                    accessPassword = account.AccessPassword;
                }
            }

            // Obtener config de plataforma para labelUsuario
            string  serviceName  = original.ServiceName ?? "IPTV";
            string  plataforma   = "IPTV";
            string? labelUsuario = "Usuario";

            if (original.TipoServicioId.HasValue)
            {
                var st = await _serviceTypeRepository.GetByIdAsync(original.TipoServicioId.Value);
                if (st is not null)
                {
                    serviceName = st.Name;
                    plataforma  = st.Plataforma;
                }
            }

            var configs = await _plataformaConfigRepo.GetAllAsync();
            var config  = configs.FirstOrDefault(c => c.Plataforma == plataforma);
            labelUsuario    = config?.LabelUsuario ?? "Usuario";
            var tienePin    = config?.TienePin ?? false;

            // Generar imagen de factura para adjuntar al WhatsApp
            string? invoiceImageUrl = null;
            if (invoiceId.HasValue)
            {
                try
                {
                    var imgResult = await _pdfTicket.GenerateTicketImageAsync(invoiceId.Value);
                    using var stream = new MemoryStream(imgResult.Content);
                    invoiceImageUrl = await _storage.UploadFileAsync(
                        stream,
                        $"factura-{imgResult.OrderNumber}.png",
                        "facturas");
                }
                catch { /* La imagen no debe bloquear la notificación */ }
            }

            // WhatsApp
            if (!string.IsNullOrWhiteSpace(customer.Phone))
            {
                try
                {
                    await _whatsApp.SendCredentialsAsync(
                        customer.Phone, customer.Name,
                        accessUser,
                        accessPassword,
                        original.PinCode, dto.NewExpiration,
                        serviceName, labelUsuario, tienePin,
                        invoiceImageUrl: invoiceImageUrl);
                }
                catch { }
            }

            // Email de respaldo
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                try
                {
                    await _email.SendCredentialsAsync(
                        customer.Email, customer.Name,
                        accessUser,
                        accessPassword,
                        original.PinCode, dto.NewExpiration,
                        serviceName, labelUsuario,
                        original.PlatformUrl,
                        isRenewal: true);
                }
                catch { }
            }
        }

        return new RenewalResponseDto
        {
            NewSubscriptionId = newSubId,
            OldSubscriptionId = subscriptionId,
            NewExpiration     = dto.NewExpiration,
            InvoiceId         = invoiceId,
            Message           = "Suscripcion renovada exitosamente."
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Guid> GenerateInvoiceAsync(
        Guid     userId,
        Guid     subscriptionId,
        Guid     customerId,
        string?  customerName,
        Guid?    tipoServicioId,
        string?  paymentMethod,
        Guid?    metodoPagoId,
        string?  paymentReference,
        decimal? amountReceived,
        decimal  discountAmount,
        bool     isRenewal = false)
    {
        decimal precio   = 0;
        string  concepto = isRenewal ? "Renovacion de Servicio IPTV" : "Servicio IPTV";

        if (tipoServicioId.HasValue)
        {
            var serviceType = await _serviceTypeRepository.GetByIdAsync(tipoServicioId.Value);
            if (serviceType != null)
            {
                precio   = serviceType.Price;
                concepto = isRenewal ? $"Renovacion: {serviceType.Name}" : serviceType.Name;
            }
        }

        var total   = precio - discountAmount;
        var invoice = new Invoice
        {
            CustomerName     = customerName ?? string.Empty,
            CustomerId       = customerId,
            UserId           = userId,
            OrderType        = isRenewal ? "Renovacion" : "Venta",
            PaymentMethod    = paymentMethod,
            MetodoPagoId     = metodoPagoId,
            PaymentReference = paymentReference,
            AmountReceived   = amountReceived ?? total,
            DiscountAmount   = discountAmount,
            TotalAmount      = total,
            SubscriptionId   = subscriptionId,
            Details          =
            [
                new InvoiceDetail
                {
                    TipoServicioId = tipoServicioId,
                    Concept        = concepto,
                    Quantity       = 1,
                    UnitPrice      = precio,
                    DiscountAmount = discountAmount,
                    SubTotal       = total
                }
            ]
        };

        return await _invoiceRepository.CreateInvoiceAsync(invoice);
    }

    private static CustomerSubscriptionResponseDto MapToDto(CustomerSubscription s) => new()
    {
        Id               = s.Id,
        CustomerId       = s.CustomerId,
        CustomerName     = s.CustomerName,
        TipoServicioId   = s.TipoServicioId,
        ServiceName      = s.ServiceName,
        SubscriptionType = s.SubscriptionType,
        PlatformUrl      = s.PlatformUrl,
        AccessUser       = s.AccessUser,
        AccessPassword   = s.AccessPassword,
        PinCode          = s.PinCode,
        StartDate        = s.StartDate,
        ExpirationDate   = s.ExpirationDate,
        Status           = s.Status,
        CreatedAt        = s.CreatedAt
    };
}
