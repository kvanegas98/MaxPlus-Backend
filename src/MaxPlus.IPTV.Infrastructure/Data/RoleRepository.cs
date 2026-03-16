using System.Data;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class RoleRepository : IRoleRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public RoleRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<dynamic>(
            "sp_Roles_ObtenerTodos",
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => new Role
        {
            Id          = (Guid)r.Id,
            Name        = (string)r.Nombre,
            Description = (string)(r.Descripcion ?? string.Empty)
        });
    }
}
