using System.Data;
using Dapper;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class OrderRepository : IOrderRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public OrderRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(Guid Id, string NumeroOrden)> AddAsync(ServiceOrder order)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerName",   order.CustomerName);
        parameters.Add("@CustomerPhone",  order.CustomerPhone);
        parameters.Add("@CustomerEmail",  order.CustomerEmail);
        parameters.Add("@TipoServicioId", order.TipoServicioId);
        parameters.Add("@Notes",          order.Notes);
        parameters.Add("@IpAddress",      order.IpAddress);
        parameters.Add("@Id",          dbType: DbType.Guid,   direction: ParameterDirection.Output);
        parameters.Add("@NumeroOrden", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

        await connection.ExecuteAsync(
            "sp_ServiceOrders_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return (parameters.Get<Guid>("@Id"), parameters.Get<string>("@NumeroOrden"));
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await connection.QueryFirstOrDefaultAsync<ServiceOrder>(
            "sp_ServiceOrders_ObtenerPorId",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ServiceOrder>> GetAllAsync(string? status = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Status", status);
        return await connection.QueryAsync<ServiceOrder>(
            "sp_ServiceOrders_ObtenerTodos",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task ApproveAsync(Guid id, Guid approvedBy, Guid subscriptionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",             id);
        parameters.Add("@ApprovedBy",     approvedBy);
        parameters.Add("@SubscriptionId", subscriptionId);

        await connection.ExecuteAsync(
            "sp_ServiceOrders_Aprobar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task RejectAsync(Guid id, Guid approvedBy, string? reason)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",              id);
        parameters.Add("@ApprovedBy",      approvedBy);
        parameters.Add("@RejectionReason", reason);

        await connection.ExecuteAsync(
            "sp_ServiceOrders_Rechazar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> AddItemAsync(ServiceOrderItem item)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ServiceOrderId", item.ServiceOrderId);
        parameters.Add("@TipoServicioId", item.TipoServicioId);
        parameters.Add("@DurationMonths", item.DurationMonths);
        parameters.Add("@Cantidad",       item.Cantidad);
        parameters.Add("@UnitPrice",      item.UnitPrice);
        parameters.Add("@SubTotal",       item.SubTotal);
        parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_ServiceOrderItems_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task<IEnumerable<ServiceOrderItem>> GetItemsByOrderIdAsync(Guid orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ServiceOrderId", orderId);
        return await connection.QueryAsync<ServiceOrderItem>(
            "sp_ServiceOrderItems_ObtenerPorOrden",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task AssignItemSubscriptionAsync(Guid itemId, Guid subscriptionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",             itemId);
        parameters.Add("@SubscriptionId", subscriptionId);
        await connection.ExecuteAsync(
            "sp_ServiceOrderItems_AsignarSuscripcion",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
