using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using System.IO;

namespace MaxPlus.IPTV.Application.Services;

public class IptvAccountService : IIptvAccountService
{
    private readonly IIptvAccountRepository      _repository;
    private readonly ICustomerRepository         _customerRepository;
    private readonly IInvoiceRepository          _invoiceRepository;
    private readonly IServiceTypeRepository      _serviceTypeRepository;
    private readonly IPlataformaConfigRepository _plataformaConfigRepo;
    private readonly IWhatsAppService            _whatsApp;
    private readonly IEmailService               _email;
    private readonly IPdfTicketService           _pdfTicket;
    private readonly IStorageService             _storage;

    public IptvAccountService(
        IIptvAccountRepository      repository,
        ICustomerRepository         customerRepository,
        IInvoiceRepository          invoiceRepository,
        IServiceTypeRepository      serviceTypeRepository,
        IPlataformaConfigRepository plataformaConfigRepo,
        IWhatsAppService            whatsApp,
        IEmailService               email,
        IPdfTicketService           pdfTicket,
        IStorageService             storage)
    {
        _repository            = repository;
        _customerRepository    = customerRepository;
        _invoiceRepository     = invoiceRepository;
        _serviceTypeRepository = serviceTypeRepository;
        _plataformaConfigRepo  = plataformaConfigRepo;
        _whatsApp              = whatsApp;
        _email                 = email;
        _pdfTicket             = pdfTicket;
        _storage               = storage;
    }

    // ── Consultas ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<IptvAccountResponseDto>> GetAllAsync()
    {
        var accounts = await _repository.GetAllAsync();
        return accounts.Select(MapToDto);
    }

    public async Task<PagedResult<IptvAccountResponseDto>> GetAllWithClientsAsync(int page, int pageSize)
    {
        var rows    = await _repository.GetWithClientsAsync();
        var grouped = GroupRows(rows)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToList();

        var total = grouped.Count;
        var data  = grouped
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);

        return new PagedResult<IptvAccountResponseDto>
        {
            Data     = data,
            Total    = total,
            Page     = page,
            PageSize = pageSize
        };
    }

    public async Task<IptvAccountResponseDto> GetWithClientsAsync(Guid id)
    {
        var rows = await _repository.GetWithClientsAsync();
        return GroupRows(rows).FirstOrDefault(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Suscripcion {id} no encontrada.");
    }

    public async Task<IEnumerable<IptvAccountAvailableDto>> GetByServiceTypeAsync(Guid tipoServicioId)
    {
        var accounts = await _repository.GetByServiceTypeAsync(tipoServicioId);
        return accounts.Select(a => new IptvAccountAvailableDto
        {
            Id             = a.Id,
            AccessUser     = a.AccessUser,
            ServiceName    = a.ServiceName,
            MaxSlots       = a.MaxSlots,
            UsedSlots      = a.UsedSlots,
            AvailableSlots = a.MaxSlots - a.UsedSlots,
            ExpirationDate = a.ExpirationDate,
            DaysRemaining  = a.DaysRemaining
        });
    }

    // ── CRUD ───────────────────────────────────────────────────────────────

    public async Task<IptvAccountResponseDto> CreateAsync(IptvAccountCreateDto dto)
    {
        var account = new IptvAccount
        {
            AccessUser     = dto.AccessUser.Trim(),
            AccessPassword = dto.AccessPassword.Trim(),
            AccessEmail    = dto.AccessEmail?.Trim(),
            PlatformUrl    = dto.PlatformUrl?.Trim(),
            PinCode        = dto.PinCode?.Trim(),
            TipoServicioId = dto.TipoServicioId,
            MaxSlots       = dto.MaxSlots,
            PurchasePrice  = dto.PurchasePrice,
            ExpirationDate = dto.ExpirationDate,
            Notes          = dto.Notes?.Trim()
        };

        account.Id = await _repository.AddAsync(account);
        return MapToDto(account);
    }

    public async Task<IptvAccountResponseDto> UpdateAsync(Guid id, IptvAccountUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Suscripcion {id} no encontrada.");

        existing.AccessUser     = dto.AccessUser.Trim();
        existing.AccessPassword = dto.AccessPassword.Trim();
        existing.AccessEmail    = dto.AccessEmail?.Trim();
        existing.PlatformUrl    = dto.PlatformUrl?.Trim();
        existing.PinCode        = dto.PinCode?.Trim();
        existing.TipoServicioId = dto.TipoServicioId;
        existing.MaxSlots       = dto.MaxSlots;
        existing.PurchasePrice  = dto.PurchasePrice;
        existing.ExpirationDate = dto.ExpirationDate;
        existing.Notes          = dto.Notes?.Trim();

        await _repository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task<IptvAccountStatsDto> GetStatsAsync()
    {
        var (totalAccounts, totalSlots, usedSlots) = await _repository.GetStatsAsync();
        return new IptvAccountStatsDto
        {
            TotalAccounts  = totalAccounts,
            TotalSlots     = totalSlots,
            UsedSlots      = usedSlots,
            AvailableSlots = totalSlots - usedSlots
        };
    }

    public async Task DeactivateAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Suscripcion {id} no encontrada.");

        await _repository.DeactivateAsync(existing.Id);
    }

    // ── Asignar cliente a un slot ──────────────────────────────────────────

    public async Task<IptvAccountAssignResponseDto> AssignClientAsync(
        Guid accountId, IptvAccountAssignClientDto dto, Guid userId)
    {
        var account = await _repository.GetByIdAsync(accountId)
            ?? throw new KeyNotFoundException($"Suscripcion {accountId} no encontrada.");

        // Resolver cliente (existente o nuevo)
        Guid    customerId;
        string? resolvedName;
        string? resolvedPhone;
        string? resolvedEmail;

        if (dto.CustomerId.HasValue && dto.CustomerId.Value != Guid.Empty)
        {
            customerId   = dto.CustomerId.Value;
            // Cargar datos del cliente desde BD para tener nombre, teléfono y email actualizados
            var existingCustomer = await _customerRepository.GetByIdAsync(customerId);
            resolvedName         = existingCustomer?.Name  ?? dto.CustomerName;
            resolvedPhone        = existingCustomer?.Phone ?? dto.CustomerPhone;
            resolvedEmail        = existingCustomer?.Email ?? dto.CustomerEmail;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                throw new InvalidOperationException("Debe enviar CustomerId o CustomerName para asignar el cliente.");

            var existing = await _customerRepository.FindByContactAsync(dto.CustomerEmail, dto.CustomerPhone);
            if (existing != null)
            {
                customerId    = existing.Id;
                resolvedName  = existing.Name;
                resolvedPhone = existing.Phone ?? dto.CustomerPhone;
                resolvedEmail = existing.Email ?? dto.CustomerEmail;
            }
            else
            {
                customerId = await _customerRepository.AddAsync(new Customer
                {
                    Name  = dto.CustomerName.Trim(),
                    Phone = dto.CustomerPhone?.Trim(),
                    Email = dto.CustomerEmail?.Trim()
                });
                resolvedName  = dto.CustomerName.Trim();
                resolvedPhone = dto.CustomerPhone?.Trim();
                resolvedEmail = dto.CustomerEmail?.Trim();
            }
        }

        // Calcular fecha y precio desde TipoServicio:
        // prioridad: dto.TipoServicioId → account.TipoServicioId → defaults
        decimal salePrice      = 0;
        string  concepto       = "Servicio IPTV";
        string  plataforma     = "IPTV";
        var     expiration     = dto.ExpirationDate ?? DateTime.UtcNow.AddDays(30);
        var     tipoServicioId = dto.TipoServicioId ?? account.TipoServicioId;

        if (tipoServicioId.HasValue)
        {
            var st = await _serviceTypeRepository.GetByIdAsync(tipoServicioId.Value);
            if (st != null)
            {
                salePrice  = st.Price;
                concepto   = st.Name;
                plataforma = st.Plataforma;
                if (dto.ExpirationDate is null)
                    expiration = DateTime.UtcNow.AddDays(st.DurationDays);
            }
        }

        // Crear slot (el SP valida que haya slots disponibles)
        var newSubId = await _repository.AssignClientAsync(
            accountId, customerId, expiration, tipoServicioId,
            dto.ProfileUser?.Trim(), dto.ProfilePin?.Trim());

        // Generar factura si hay método de pago
        Guid? invoiceId = null;
        if (!string.IsNullOrWhiteSpace(dto.PaymentMethod) || dto.MetodoPagoId.HasValue)
            invoiceId = await GenerateInvoiceAsync(
                userId, newSubId, customerId, resolvedName,
                tipoServicioId, salePrice, concepto, dto.DiscountAmount,
                dto.PaymentMethod, dto.MetodoPagoId,
                dto.PaymentReference, dto.AmountReceived);

        // Enviar credenciales por WhatsApp si el cliente tiene teléfono
        var phone = resolvedPhone?.Trim();
        if (!string.IsNullOrWhiteSpace(phone))
        {
            try
            {
                var configs      = await _plataformaConfigRepo.GetAllAsync();
                var config       = configs.FirstOrDefault(c => c.Plataforma == plataforma);
                var labelUsuario = config?.LabelUsuario ?? "Usuario";
                var tienePin     = config?.TienePin     ?? false;

                // Netflix/Streaming: el "usuario" principal es el correo de la cuenta;
                // el perfil individual (usuario + pin) se envía aparte.
                // IPTV/FlujoTV: usuario de acceso directo, sin perfil.
                var whatsappUser = !string.IsNullOrWhiteSpace(account.AccessEmail)
                    ? account.AccessEmail
                    : account.AccessUser;

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
                    catch { /* La imagen no debe bloquear el envío */ }
                }

                await _whatsApp.SendCredentialsAsync(
                    phone, resolvedName ?? string.Empty,
                    whatsappUser, account.AccessPassword,
                    account.PinCode, expiration,
                    concepto, labelUsuario, tienePin,
                    profileUser:     dto.ProfileUser?.Trim(),
                    profilePin:      dto.ProfilePin?.Trim(),
                    invoiceImageUrl: invoiceImageUrl);
            }
            catch { /* No bloquear la asignación si falla WhatsApp */ }
        }

        // Email de respaldo — solo si el cliente tiene correo
        if (!string.IsNullOrWhiteSpace(resolvedEmail))
        {
            try
            {
                var whatsappUserEmail = !string.IsNullOrWhiteSpace(account.AccessEmail)
                    ? account.AccessEmail
                    : account.AccessUser;

                var configs      = await _plataformaConfigRepo.GetAllAsync();
                var config       = configs.FirstOrDefault(c => c.Plataforma == plataforma);
                var labelUsuario = config?.LabelUsuario ?? "Usuario";

                await _email.SendCredentialsAsync(
                    resolvedEmail, resolvedName ?? string.Empty,
                    whatsappUserEmail, account.AccessPassword,
                    account.PinCode, expiration,
                    concepto, labelUsuario,
                    account.PlatformUrl,
                    dto.ProfileUser?.Trim(),
                    dto.ProfilePin?.Trim());
            }
            catch { /* El email no debe bloquear la asignación */ }
        }

        return new IptvAccountAssignResponseDto
        {
            SubscriptionId = newSubId,
            AccountId      = accountId,
            CustomerName   = resolvedName ?? string.Empty,
            ExpirationDate = expiration,
            InvoiceId      = invoiceId,
            Message        = "Cliente asignado exitosamente."
        };
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static IEnumerable<IptvAccountResponseDto> GroupRows(IEnumerable<IptvAccountSlotRow> rows)
    {
        return rows
            .GroupBy(r => r.AccountId)
            .Select(g =>
            {
                var first = g.First();

                var dto = new IptvAccountResponseDto
                {
                    Id             = first.AccountId,
                    AccessUser     = first.AccessUser,
                    AccessPassword = first.AccessPassword,
                    PlatformUrl    = first.PlatformUrl,
                    PinCode        = first.PinCode,
                    TipoServicioId = first.TipoServicioId,
                    ServiceName    = first.ServiceName,
                    MaxSlots       = first.MaxSlots,
                    PurchasePrice  = first.PurchasePrice,
                    ExpirationDate = first.AccountExpirationDate,
                    DaysRemaining  = first.AccountDaysRemaining,
                    Notes          = first.Notes,
                    IsActive       = true,
                    CreatedAt      = first.CreatedAt,
                    Clients        = []
                };

                var assignedSlots = g
                    .Where(r => r.SubscriptionId.HasValue)
                    .Select(r => new IptvAccountSlotDto
                    {
                        SubscriptionId = r.SubscriptionId,
                        CustomerId     = r.CustomerId,
                        CustomerName   = r.CustomerName,
                        CustomerPhone  = r.CustomerPhone,
                        CustomerEmail  = r.CustomerEmail,
                        ProfileUser    = r.ProfileUser,
                        ProfilePin     = r.ProfilePin,
                        StartDate      = r.StartDate,
                        ExpirationDate = r.ExpirationDate,
                        DaysRemaining  = r.DaysRemaining,
                        Status         = r.SlotStatus ?? "Active"
                    })
                    .ToList();

                dto.Clients.AddRange(assignedSlots);

                int emptySlots = first.MaxSlots - assignedSlots.Count;
                for (int i = 0; i < emptySlots; i++)
                    dto.Clients.Add(new IptvAccountSlotDto { Status = "Available" });

                dto.UsedSlots      = assignedSlots.Count;
                dto.AvailableSlots = emptySlots;

                return dto;
            });
    }

    private static IptvAccountResponseDto MapToDto(IptvAccount a) => new()
    {
        Id             = a.Id,
        AccessUser     = a.AccessUser,
        AccessPassword = a.AccessPassword,
        AccessEmail    = a.AccessEmail,
        PlatformUrl    = a.PlatformUrl,
        PinCode        = a.PinCode,
        TipoServicioId = a.TipoServicioId,
        ServiceName    = a.ServiceName,
        MaxSlots       = a.MaxSlots,
        PurchasePrice  = a.PurchasePrice,
        ExpirationDate = a.ExpirationDate,
        DaysRemaining  = a.DaysRemaining,
        Notes          = a.Notes,
        IsActive       = a.IsActive,
        CreatedAt      = a.CreatedAt,
        UsedSlots      = a.UsedSlots,
        AvailableSlots = a.MaxSlots - a.UsedSlots,
        Clients        = []
    };

    private async Task<Guid> GenerateInvoiceAsync(
        Guid     userId,
        Guid     subscriptionId,
        Guid     customerId,
        string?  customerName,
        Guid?    tipoServicioId,
        decimal  salePrice,
        string   concepto,
        decimal  discountAmount,
        string?  paymentMethod,
        Guid?    metodoPagoId,
        string?  paymentReference,
        decimal? amountReceived)
    {
        var total   = salePrice - discountAmount;
        var invoice = new Invoice
        {
            CustomerName     = customerName ?? string.Empty,
            CustomerId       = customerId,
            UserId           = userId,
            OrderType        = "Venta",
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
                    UnitPrice      = salePrice,
                    DiscountAmount = discountAmount,
                    SubTotal       = total
                }
            ]
        };

        return await _invoiceRepository.CreateInvoiceAsync(invoice);
    }
}
