using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IReportService
{
    Task<ReportSummaryDto>              GetSummaryAsync(string filter);
    Task<IEnumerable<HourlySaleDto>>    GetSalesByHourAsync();
    Task<IEnumerable<TopProductDto>>    GetTopProductsAsync(string filter, int top = 10);
    Task<IEnumerable<OrderHistoryDto>>  GetOrderHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 10, string? customerName = null, string? orderNumber = null);
    Task VoidInvoiceAsync(Guid invoiceId, Guid userId, string? reason);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId);
}
