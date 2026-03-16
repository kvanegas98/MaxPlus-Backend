using System.Data;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class DashboardRepository : IDashboardRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DashboardRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DashboardSummary> GetSummaryAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@FechaDesde", fechaDesde.Date);
        parameters.Add("@FechaHasta", fechaHasta.Date);

        using var multi = await connection.QueryMultipleAsync(
            "sp_Dashboard_ResumenFinanciero",
            parameters,
            commandType: CommandType.StoredProcedure);

        var summary = await multi.ReadSingleAsync<DashboardSummary>();
        summary.Monthly = (await multi.ReadAsync<DashboardMonthly>()).ToList();
        return summary;
    }

    public async Task<IEnumerable<DashboardExpiring>> GetExpiringAsync(int daysAhead)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@DaysAhead", daysAhead);
        return await connection.QueryAsync<DashboardExpiring>(
            "sp_Dashboard_PorVencer",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
