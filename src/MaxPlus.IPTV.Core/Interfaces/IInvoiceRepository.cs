using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IInvoiceRepository
{
    Task<Guid> CreateInvoiceAsync(Invoice invoice);
    Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId);
    Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId);
    Task VoidInvoiceAsync(Guid invoiceId, Guid userId, string? reason);
}
