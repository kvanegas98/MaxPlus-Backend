using System.Data;
using Dapper;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class IptvAccountRepository : IIptvAccountRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public IptvAccountRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<IptvAccount>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<IptvAccount>(
            "sp_IptvAccounts_ObtenerTodos",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IptvAccount?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await connection.QuerySingleOrDefaultAsync<IptvAccount>(
            "sp_IptvAccounts_ObtenerPorId",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<IptvAccountSlotRow>> GetWithClientsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<IptvAccountSlotRow>(
            "sp_IptvAccounts_ObtenerConClientes",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<IptvAccount>> GetByServiceTypeAsync(Guid tipoServicioId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@TipoServicioId", tipoServicioId);
        return await connection.QueryAsync<IptvAccount>(
            "sp_IptvAccounts_PorTipoServicio",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> AddAsync(IptvAccount account)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@AccessUser",     account.AccessUser);
        parameters.Add("@AccessPassword", account.AccessPassword);
        parameters.Add("@AccessEmail",    account.AccessEmail);
        parameters.Add("@PlatformUrl",    account.PlatformUrl);
        parameters.Add("@PinCode",        account.PinCode);
        parameters.Add("@TipoServicioId", account.TipoServicioId);
        parameters.Add("@MaxSlots",       account.MaxSlots);
        parameters.Add("@PurchasePrice",  account.PurchasePrice);
        parameters.Add("@ExpirationDate", account.ExpirationDate);
        parameters.Add("@Notes",          account.Notes);
        parameters.Add("@Id",             dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_IptvAccounts_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task UpdateAsync(IptvAccount account)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",             account.Id);
        parameters.Add("@AccessUser",     account.AccessUser);
        parameters.Add("@AccessPassword", account.AccessPassword);
        parameters.Add("@AccessEmail",    account.AccessEmail);
        parameters.Add("@PlatformUrl",    account.PlatformUrl);
        parameters.Add("@PinCode",        account.PinCode);
        parameters.Add("@TipoServicioId", account.TipoServicioId);
        parameters.Add("@MaxSlots",       account.MaxSlots);
        parameters.Add("@PurchasePrice",  account.PurchasePrice);
        parameters.Add("@ExpirationDate", account.ExpirationDate);
        parameters.Add("@Notes",          account.Notes);

        await connection.ExecuteAsync(
            "sp_IptvAccounts_Actualizar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeactivateAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        await connection.ExecuteAsync(
            "sp_IptvAccounts_Desactivar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> AssignClientAsync(Guid accountId, Guid customerId, DateTime expirationDate, Guid? tipoServicioId, string? profileUser, string? profilePin)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@IptvAccountId",  accountId);
        parameters.Add("@CustomerId",     customerId);
        parameters.Add("@ExpirationDate", expirationDate);
        parameters.Add("@TipoServicioId", tipoServicioId);
        parameters.Add("@ProfileUser",    profileUser);
        parameters.Add("@ProfilePin",     profilePin);
        parameters.Add("@NewSubId",       dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_IptvAccounts_AsignarCliente",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@NewSubId");
    }

    public async Task<(int totalAccounts, int totalSlots, int usedSlots)> GetStatsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync(
            "sp_IptvAccounts_Stats",
            commandType: CommandType.StoredProcedure);

        return ((int)result.TotalAccounts, (int)result.TotalSlots, (int)result.UsedSlots);
    }
}
