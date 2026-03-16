using Dapper;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using System.Data;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class SystemLogRepository : ISystemLogRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SystemLogRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task LogAsync(string level, string source, string message, string? details = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Level",   level);
        parameters.Add("@Source",  source);
        parameters.Add("@Message", message);
        parameters.Add("@Details", details);

        await connection.ExecuteAsync(
            "sp_Logs_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SystemLog>> GetAllAsync(string? level = null, string? source = null, int top = 200)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Level",  level);
        parameters.Add("@Source", source);
        parameters.Add("@Top",    top);

        return await connection.QueryAsync<SystemLog>(
            "sp_Logs_ObtenerTodos",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
