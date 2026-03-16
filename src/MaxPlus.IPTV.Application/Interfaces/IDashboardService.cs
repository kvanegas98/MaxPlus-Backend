using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>Resumen financiero + operativo para el período dado (default: mes actual).</summary>
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime? fechaDesde, DateTime? fechaHasta);

    /// <summary>Suscripciones que vencen en los próximos N días.</summary>
    Task<IEnumerable<DashboardExpiringDto>> GetExpiringAsync(int daysAhead = 30);
}
