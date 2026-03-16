using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummary>              GetSummaryAsync(DateTime fechaDesde, DateTime fechaHasta);
    Task<IEnumerable<DashboardExpiring>> GetExpiringAsync(int daysAhead);
}
