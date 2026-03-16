using System.Data;
using MaxPlus.IPTV.Application.Helpers;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "sp_Usuarios_ObtenerPorEmail",
            new { Email = email },
            commandType: CommandType.StoredProcedure);

        return row is null ? null : MapToUser(row);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "sp_Usuarios_ObtenerPorId",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        return row is null ? null : MapToUser(row);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(
            "sp_Usuarios_ObtenerTodos",
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => (User)MapToUser(r));
    }

    public async Task<Guid> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@FullName",     user.FullName);
        parameters.Add("@Email",        user.Email);
        parameters.Add("@PasswordHash", user.PasswordHash);
        parameters.Add("@RoleId",       user.RoleId);
        parameters.Add("@IsActive",     user.IsActive);
        parameters.Add("@Id",           dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_Usuarios_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_Usuarios_Actualizar",
            new
            {
                Id             = user.Id,
                FullName = user.FullName,
                Email    = user.Email,
                RoleId   = user.RoleId,
                IsActive = user.IsActive
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task ChangePasswordAsync(Guid id, string newHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_Usuarios_CambiarPassword",
            new { Id = id, PasswordHash = newHash },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateLastLoginAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_Usuarios_ActualizarUltimoLogin",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeactivateAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_Usuarios_DesactivarPorId",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    private static User MapToUser(dynamic r) => new()
    {
        Id           = (Guid)r.Id,
        FullName     = (string)r.FullName,
        Email        = (string)r.Email,
        PasswordHash = r.PasswordHash is string h ? h : string.Empty,
        RoleId       = (Guid)r.RoleId,
        RoleName     = (string)r.RoleName,
        IsActive     = r.IsActive is bool b ? b : (bool)(r.IsActive == 1),
        CreatedAt    = r.CreatedAt is DateTime c ? c : DateTimeHelper.GetNicaraguaTime(),
        LastLoginAt  = r.LastLoginAt is DateTime l ? l : (DateTime?)null
    };
}
