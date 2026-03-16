using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository  _reportRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public ReportService(IReportRepository reportRepository, IInvoiceRepository invoiceRepository)
    {
        _reportRepository  = reportRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<ReportSummaryDto> GetSummaryAsync(string filter)
    {
        var data = await _reportRepository.GetSummaryAsync(NormalizeFilter(filter));

        return new ReportSummaryDto
        {
            TotalSales          = data.VentasTotales,
            AverageTicket       = data.TicketPromedio,
            TotalOrders         = data.TotalOrdenes,
            TopSellingProduct   = data.ProductoMasVendido,
            ByOrderType         = data.ByOrderType.Select(t => new OrderTypeCountDto
            {
                OrderType   = t.TipoOrden,
                TotalOrders = t.TotalOrdenes
            }).ToList(),
            TotalDemosToday     = data.TotalDemosHoy,
            ActiveSubscriptions = data.SuscripcionesActivas
        };
    }

    public async Task<IEnumerable<HourlySaleDto>> GetSalesByHourAsync()
    {
        var rows = await _reportRepository.GetSalesByHourAsync();
        return rows.Select(r => new HourlySaleDto
        {
            Hour        = r.Hour,
            TotalOrders = r.TotalOrders,
            TotalSales  = r.TotalSales
        }).ToList();
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(string filter, int top = 10)
    {
        var rows = await _reportRepository.GetTopProductsAsync(NormalizeFilter(filter), top);
        return rows.Select(r => new TopProductDto
        {
            Name         = r.Name,
            UnitsSold    = r.UnitsSold,
            TotalRevenue = r.TotalRevenue
        }).ToList();
    }

    public async Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 10, string? customerName = null, string? orderNumber = null)
    {
        var end = endDate?.Date.AddDays(1).AddSeconds(-1) ?? DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        var start = startDate?.Date ?? end.AddDays(-30).Date;

        var rows = await _reportRepository.GetOrderHistoryAsync(start, end, pageNumber, pageSize, customerName, orderNumber);
        return rows.Select(r => new OrderHistoryDto
        {
            InvoiceId            = r.InvoiceId,
            NumeroOrden          = r.NumeroOrden,
            SaleDate             = r.SaleDate,
            OrderType            = r.OrderType,
            PaymentMethod        = r.PaymentMethod,
            TotalAmount          = r.TotalAmount,
            Status               = r.Status,
            CustomerName         = r.CustomerName,
            RegisteredClientName = r.RegisteredClientName,
            TotalProducts        = r.TotalProducts,
            Details = r.Details.Select(d => new InvoiceDetailResponseDto
            {
                Concept        = d.Concept,
                Quantity       = d.Quantity,
                UnitPrice      = d.UnitPrice,
                DiscountAmount = d.DiscountAmount,
                SubTotal       = d.SubTotal
            }).ToList()
        }).ToList();
    }

    public async Task VoidInvoiceAsync(Guid invoiceId, Guid userId, string? reason)
    {
        await _reportRepository.VoidInvoiceAsync(invoiceId, userId, reason);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        if (invoice is null) return null;

        return new InvoiceResponseDto
        {
            Id              = invoice.Id,
            NumeroOrden     = invoice.NumeroOrden ?? string.Empty,
            SaleDate        = invoice.SaleDate,
            CustomerName    = invoice.CustomerName,
            CustomerId      = invoice.CustomerId,
            OrderType       = invoice.OrderType,
            PaymentMethod   = invoice.PaymentMethod,
            MetodoPagoId    = invoice.MetodoPagoId,
            PaymentReference = invoice.PaymentReference,
            AmountReceived  = invoice.AmountReceived,
            DiscountAmount  = invoice.DiscountAmount,
            TotalAmount     = invoice.TotalAmount,
            Status          = invoice.Status,
            Nota            = invoice.Nota,
            SubscriptionId  = invoice.SubscriptionId,
            Details         = invoice.Details.Select(d => new InvoiceDetailResponseDto
            {
                Concept        = d.Concept,
                Quantity       = d.Quantity,
                UnitPrice      = d.UnitPrice,
                DiscountAmount = d.DiscountAmount,
                SubTotal       = d.SubTotal,
                Nota           = d.Nota
            }).ToList()
        };
    }

    private static string NormalizeFilter(string filter) =>
        filter?.Trim().ToLower() == "semana" ? "Semana" : "Hoy";
}
