using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IReportRepository
{
    Task<ReportSummaryData>              GetSummaryAsync(string filter);
    Task<IEnumerable<HourlySaleRow>>     GetSalesByHourAsync();
    Task<IEnumerable<TopProductRow>>     GetTopProductsAsync(string filter, int top = 10);
    Task<IEnumerable<OrderHistoryRow>>   GetOrderHistoryAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10, string? customerName = null, string? orderNumber = null);
    Task VoidInvoiceAsync(Guid invoiceId, Guid userId, string? reason);
}

// ── Tipos de resultado ────────────────────────────────────────────────────────

public record ReportSummaryData(
    decimal VentasTotales,
    decimal TicketPromedio,
    int     TotalOrdenes,
    string? ProductoMasVendido,
    IEnumerable<OrderTypeCount>   ByOrderType,
    int     TotalDemosHoy,
    int     SuscripcionesActivas
);

public record OrderTypeCount(
    string TipoOrden,
    int    TotalOrdenes
);

public record HourlySaleRow(
    int     Hour,
    int     TotalOrders,
    decimal TotalSales
);

public record TopProductRow(
    string  Name,
    int     UnitsSold,
    decimal TotalRevenue
);

public record OrderHistoryRow(
    Guid     InvoiceId,
    string   NumeroOrden,
    DateTime SaleDate,
    string   OrderType,
    string?  PaymentMethod,
    decimal  TotalAmount,
    string   Status,
    string   CustomerName,
    string?  RegisteredClientName,
    int      TotalProducts,
    IEnumerable<InvoiceDetail> Details
);
