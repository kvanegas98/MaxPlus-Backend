namespace MaxPlus.IPTV.Application.DTOs;

public class DashboardSummaryDto
{
    // Financiero
    public decimal Ingresos         { get; set; }
    public decimal Costos           { get; set; }
    public decimal Ganancia         { get; set; }
    public int     TotalFacturas    { get; set; }
    public int     CuentasCompradas { get; set; }

    // Operativo
    public int ActiveSubscriptions  { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int ExpiringIn7Days      { get; set; }
    public int ExpiringIn30Days     { get; set; }

    // Período consultado
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }

    // Detalle mensual para gráfica
    public List<DashboardMonthlyDto> Monthly { get; set; } = [];
}

public class DashboardMonthlyDto
{
    public string  Mes       { get; set; } = string.Empty;  // "2026-03"
    public decimal Ingresos  { get; set; }
    public int     Facturas  { get; set; }
}

public class DashboardExpiringDto
{
    public Guid     Id            { get; set; }
    public Guid?    CustomerId    { get; set; }
    public string?  CustomerName  { get; set; }
    public string?  CustomerPhone { get; set; }
    public string?  ServiceName   { get; set; }
    public string   Status        { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public int      DaysRemaining  { get; set; }
    public string?  AccessUser    { get; set; }
}
