using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class CustomerPortalService : ICustomerPortalService
{
    private readonly ICustomerRepository             _customerRepo;
    private readonly ICustomerSubscriptionRepository _subscriptionRepo;
    private readonly IDemoRequestRepository          _demoRepo;
    private readonly IInvoiceRepository              _invoiceRepo;

    public CustomerPortalService(
        ICustomerRepository             customerRepo,
        ICustomerSubscriptionRepository subscriptionRepo,
        IDemoRequestRepository          demoRepo,
        IInvoiceRepository              invoiceRepo)
    {
        _customerRepo     = customerRepo;
        _subscriptionRepo = subscriptionRepo;
        _demoRepo         = demoRepo;
        _invoiceRepo      = invoiceRepo;
    }

    public async Task<CustomerPortalResponseDto?> GetByCustomerIdAsync(Guid customerId)
    {
        // 1. Buscar cliente por ID
        var customer = await _customerRepo.GetByIdAsync(customerId);
        if (customer is null) return null;

        // 2. Suscripciones — todas (historial + activas)
        var subs = await _subscriptionRepo.GetByCustomerIdAsync(customer.Id);

        // 3. Demos solicitadas — por teléfono del cliente (puede haber cambiado, pero es lo más cercano)
        var demos = customer.Phone is not null
            ? await _demoRepo.GetByPhoneAsync(customer.Phone)
            : Enumerable.Empty<MaxPlus.IPTV.Core.Entities.DemoRequest>();

        // 4. Facturas del cliente
        var invoices = await _invoiceRepo.GetByCustomerIdAsync(customer.Id);

        return new CustomerPortalResponseDto
        {
            CustomerId    = customer.Id,
            CustomerName  = customer.Name,
            CustomerPhone = customer.Phone,
            CustomerEmail = customer.Email,

            Subscriptions = subs.Select(s =>
            {
                bool isActive = s.Status == "Active";
                return new CustomerPortalSubscriptionDto
                {
                    Id             = s.Id,
                    ServiceName    = s.ServiceName,
                    Status         = s.Status,
                    StartDate      = s.StartDate,
                    ExpirationDate = s.ExpirationDate,
                    DaysRemaining  = (int)(s.ExpirationDate.Date - DateTime.UtcNow.Date).TotalDays,
                    // Credenciales solo si activa
                    AccessUser     = isActive ? s.AccessUser     : null,
                    AccessPassword = isActive ? s.AccessPassword : null,
                    PinCode        = isActive ? s.PinCode        : null,
                    PlatformUrl    = isActive ? s.PlatformUrl    : null,
                    ProfileUser    = isActive ? s.ProfileUser    : null,
                    ProfilePin     = isActive ? s.ProfilePin     : null,
                };
            }).OrderByDescending(s => s.ExpirationDate).ToList(),

            Demos = demos.Select(d => new CustomerPortalDemoDto
            {
                Id          = d.Id,
                ServiceName = d.ServiceName,
                Status      = d.Status,
                ExpiresAt   = d.ExpiresAt,
                CreatedAt   = d.CreatedAt,
            }).OrderByDescending(d => d.CreatedAt).ToList(),

            Invoices = invoices.Select(i => new CustomerPortalInvoiceDto
            {
                Id               = i.Id,
                NumeroOrden      = i.NumeroOrden,
                TotalAmount      = i.TotalAmount,
                PaymentMethod    = i.PaymentMethod,
                PaymentReference = i.PaymentReference,
                Status           = i.Status,
                SaleDate         = i.SaleDate,
                Details          = i.Details.Select(d => new CustomerPortalInvoiceDetailDto
                {
                    Concept  = d.Concept,
                    Quantity = d.Quantity,
                    SubTotal = d.SubTotal,
                }).ToList(),
            }).OrderByDescending(i => i.SaleDate).ToList(),
        };
    }
}
