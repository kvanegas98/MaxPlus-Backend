namespace MaxPlus.IPTV.Application.DTOs;

public class ReportSummaryDto
{
    public decimal                           TotalSales            { get; set; }
    public decimal                           AverageTicket         { get; set; }
    public int                               TotalOrders           { get; set; }
    public string?                           TopSellingProduct     { get; set; }
    public IEnumerable<OrderTypeCountDto>    ByOrderType           { get; set; } = new List<OrderTypeCountDto>();
    public int                               TotalDemosToday       { get; set; }
    public int                               ActiveSubscriptions   { get; set; }
}

public class OrderTypeCountDto
{
    public string OrderType   { get; set; } = string.Empty;
    public int    TotalOrders { get; set; }
}

public class HourlySaleDto
{
    public int     Hour         { get; set; }
    public int     TotalOrders  { get; set; }
    public decimal TotalSales   { get; set; }
}

public class TopProductDto
{
    public string  Name           { get; set; } = string.Empty;
    public int     UnitsSold      { get; set; }
    public decimal TotalRevenue   { get; set; }
}

public class OrderHistoryDto
{
    public Guid     InvoiceId               { get; set; }
    public string   NumeroOrden             { get; set; } = string.Empty;
    public DateTime SaleDate                { get; set; }
    public string   OrderType               { get; set; } = string.Empty;
    public string?  PaymentMethod           { get; set; }
    public decimal  TotalAmount             { get; set; }
    public string   Status                  { get; set; } = string.Empty;
    public string   CustomerName            { get; set; } = string.Empty;
    public string?  RegisteredClientName    { get; set; }
    public int      TotalProducts           { get; set; }
    public List<InvoiceDetailResponseDto> Details { get; set; } = new();
}

public class VoidRequestDto
{
    public string? Reason { get; set; }
}
