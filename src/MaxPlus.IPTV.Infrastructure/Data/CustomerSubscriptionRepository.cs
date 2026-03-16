using System.Data;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class CustomerSubscriptionRepository : ICustomerSubscriptionRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CustomerSubscriptionRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<CustomerSubscription>> GetByCustomerIdAsync(Guid customerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerId", customerId);
        return await connection.QueryAsync<CustomerSubscription>(
            "sp_Subscriptions_ObtenerPorCliente",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CustomerSubscription>> GetActiveAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<CustomerSubscription>(
            "sp_Subscriptions_ObtenerActivas",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CustomerSubscription>> GetUnassignedAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<CustomerSubscription>(
            "sp_Subscriptions_ObtenerSinAsignar",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<CustomerSubscription?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await connection.QuerySingleOrDefaultAsync<CustomerSubscription>(
            "sp_Subscriptions_ObtenerPorId",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CustomerSubscription>> GetExpiringAsync(int daysAhead)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@DaysAhead", daysAhead);
        return await connection.QueryAsync<CustomerSubscription>(
            "sp_CustomerSubscriptions_ObtenerPorVencer",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> AddAsync(CustomerSubscription subscription)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerId",       subscription.CustomerId);
        parameters.Add("@TipoServicioId",   subscription.TipoServicioId);
        parameters.Add("@SubscriptionType", subscription.SubscriptionType);
        parameters.Add("@PlatformUrl",      subscription.PlatformUrl);
        parameters.Add("@AccessUser",       subscription.AccessUser);
        parameters.Add("@AccessPassword",   subscription.AccessPassword);
        parameters.Add("@PinCode",          subscription.PinCode);
        parameters.Add("@ExpirationDate",   subscription.ExpirationDate);
        parameters.Add("@Id",               dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_Subscriptions_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task AssignCustomerAsync(Guid id, Guid customerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",         id);
        parameters.Add("@CustomerId", customerId);
        await connection.ExecuteAsync(
            "sp_Subscriptions_AsignarCliente",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(CustomerSubscription subscription)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",             subscription.Id);
        parameters.Add("@PlatformUrl",    subscription.PlatformUrl);
        parameters.Add("@AccessUser",     subscription.AccessUser);
        parameters.Add("@AccessPassword", subscription.AccessPassword);
        parameters.Add("@PinCode",        subscription.PinCode);
        parameters.Add("@ExpirationDate", subscription.ExpirationDate);
        parameters.Add("@Status",         subscription.Status);

        await connection.ExecuteAsync(
            "sp_Subscriptions_Actualizar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task CancelAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        await connection.ExecuteAsync(
            "sp_Subscriptions_Cancelar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task MarkNotifiedAsync(Guid id, int daysAhead)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",        id);
        parameters.Add("@DaysAhead", daysAhead);
        await connection.ExecuteAsync(
            "sp_CustomerSubscriptions_MarcarNotificado",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ExpireOldAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Cantidad", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await connection.ExecuteAsync(
            "sp_CustomerSubscriptions_ExpirarVencidas",
            parameters,
            commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@Cantidad");
    }

    public async Task<Guid> RenewAsync(Guid subscriptionId, DateTime newExpiration)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@SubscriptionId", subscriptionId);
        parameters.Add("@NewExpiration",  newExpiration);
        parameters.Add("@NewId",          dbType: DbType.Guid, direction: ParameterDirection.Output);
        await connection.ExecuteAsync(
            "sp_CustomerSubscriptions_Renovar",
            parameters,
            commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NewId");
    }
}
