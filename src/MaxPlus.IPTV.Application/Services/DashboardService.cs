using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var now   = DateTime.UtcNow;
        var desde = fechaDesde ?? new DateTime(now.Year, now.Month, 1);
        var hasta = fechaHasta ?? new DateTime(now.Year, now.Month,
            DateTime.DaysInMonth(now.Year, now.Month));

        var data = await _repository.GetSummaryAsync(desde, hasta);

        return new DashboardSummaryDto
        {
            Ingresos             = data.Ingresos,
            Costos               = data.Costos,
            Ganancia             = data.Ganancia,
            TotalFacturas        = data.TotalFacturas,
            CuentasCompradas     = data.CuentasCompradas,
            ActiveSubscriptions  = data.ActiveSubscriptions,
            ExpiredSubscriptions = data.ExpiredSubscriptions,
            ExpiringIn7Days      = data.ExpiringIn7Days,
            ExpiringIn30Days     = data.ExpiringIn30Days,
            FechaDesde           = data.FechaDesde,
            FechaHasta           = data.FechaHasta,
            Monthly              = data.Monthly.Select(m => new DashboardMonthlyDto
            {
                Mes      = m.Mes,
                Ingresos = m.Ingresos,
                Facturas = m.Facturas,
            }).ToList()
        };
    }

    public async Task<IEnumerable<DashboardExpiringDto>> GetExpiringAsync(int daysAhead = 30)
    {
        var items = await _repository.GetExpiringAsync(daysAhead);
        return items.Select(e => new DashboardExpiringDto
        {
            Id             = e.Id,
            CustomerId     = e.CustomerId,
            CustomerName   = e.CustomerName,
            CustomerPhone  = e.CustomerPhone,
            ServiceName    = e.ServiceName,
            Status         = e.Status,
            ExpirationDate = e.ExpirationDate,
            DaysRemaining  = e.DaysRemaining,
            AccessUser     = e.AccessUser,
        });
    }
}
