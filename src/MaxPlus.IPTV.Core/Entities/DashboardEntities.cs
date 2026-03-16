namespace MaxPlus.IPTV.Core.Entities;

public class DashboardSummary
{
    public decimal Ingresos         { get; set; }
    public decimal Costos           { get; set; }
    public decimal Ganancia         { get; set; }
    public int     TotalFacturas    { get; set; }
    public int     CuentasCompradas { get; set; }
    public int     ActiveSubscriptions  { get; set; }
    public int     ExpiredSubscriptions { get; set; }
    public int     ExpiringIn7Days  { get; set; }
    public int     ExpiringIn30Days { get; set; }
    public DateTime FechaDesde     { get; set; }
    public DateTime FechaHasta     { get; set; }

    // Poblado desde el segundo result set en DashboardRepository
    public List<DashboardMonthly> Monthly { get; set; } = [];
}

public class DashboardMonthly
{
    public string  Mes      { get; set; } = string.Empty;
    public decimal Ingresos { get; set; }
    public int     Facturas { get; set; }
}

public class DashboardExpiring
{
    public Guid     Id             { get; set; }
    public Guid?    CustomerId     { get; set; }
    public string?  CustomerName   { get; set; }
    public string?  CustomerPhone  { get; set; }
    public string?  ServiceName    { get; set; }
    public string   Status         { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public int      DaysRemaining  { get; set; }
    public string?  AccessUser     { get; set; }
}
