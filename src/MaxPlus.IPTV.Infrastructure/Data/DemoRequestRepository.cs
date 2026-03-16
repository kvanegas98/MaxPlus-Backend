using System.Data;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class DemoRequestRepository : IDemoRequestRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DemoRequestRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> AddAsync(DemoRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerName",  request.CustomerName);
        parameters.Add("@CustomerPhone", request.CustomerPhone);
        parameters.Add("@CustomerEmail", request.CustomerEmail);
        parameters.Add("@IpAddress",     request.IpAddress);
        parameters.Add("@Country",       request.Country);
        parameters.Add("@PhoneVerificationCode", request.PhoneVerificationCode);
        parameters.Add("@TipoServicioId", request.TipoServicioId);
        parameters.Add("@Id",            dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_DemoRequests_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task<IEnumerable<DemoRequest>> GetAllAsync(string? status = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Status", status);
        return await connection.QueryAsync<DemoRequest>(
            "sp_DemoRequests_ObtenerTodos",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DemoRequest?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await connection.QueryFirstOrDefaultAsync<DemoRequest>(
            "sp_DemoRequests_ObtenerPorId",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task ApproveAsync(Guid id, Guid? approvedBy, string? demoUrl, string? responseHtml)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",           id);
        parameters.Add("@ApprovedBy",   approvedBy);
        parameters.Add("@DemoUrl",      demoUrl);
        parameters.Add("@ResponseHtml", responseHtml);

        await connection.ExecuteAsync(
            "sp_DemoRequests_Aprobar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task RejectAsync(Guid id, Guid approvedBy, string? rejectionReason)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",              id);
        parameters.Add("@ApprovedBy",      approvedBy);
        parameters.Add("@RejectionReason", rejectionReason);

        await connection.ExecuteAsync(
            "sp_DemoRequests_Rechazar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task VerifyPhoneAsync(Guid id, string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@Code", code);

        await connection.ExecuteAsync(
            "sp_DemoRequests_VerificarTelefono",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<DemoRequest>> GetByPhoneAsync(string phone)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Phone", phone);
        return await connection.QueryAsync<DemoRequest>(
            "sp_DemoRequests_PorTelefono",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
