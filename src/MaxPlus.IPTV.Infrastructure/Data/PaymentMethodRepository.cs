using Dapper;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using System.Data;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public PaymentMethodRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PaymentMethod>(
            "sp_MetodosPago_ObtenerTodos",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PaymentMethod>(
            "SELECT Id, Nombre, Banco, TipoCuenta, NumeroCuenta, Titular, IsActive, CreatedAt FROM [dbo].[MetodosPago] WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<Guid> CreateAsync(PaymentMethod pm)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Nombre",       pm.Nombre);
        parameters.Add("@Banco",        pm.Banco);
        parameters.Add("@TipoCuenta",   pm.TipoCuenta);
        parameters.Add("@NumeroCuenta", pm.NumeroCuenta);
        parameters.Add("@Titular",      pm.Titular);
        parameters.Add("@Id",           dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_MetodosPago_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task UpdateAsync(PaymentMethod pm)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",           pm.Id);
        parameters.Add("@Nombre",       pm.Nombre);
        parameters.Add("@Banco",        pm.Banco);
        parameters.Add("@TipoCuenta",   pm.TipoCuenta);
        parameters.Add("@NumeroCuenta", pm.NumeroCuenta);
        parameters.Add("@Titular",      pm.Titular);
        parameters.Add("@IsActive",     pm.IsActive);

        await connection.ExecuteAsync(
            "sp_MetodosPago_Actualizar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE [dbo].[MetodosPago] SET IsActive = 0 WHERE Id = @Id",
            new { Id = id });
    }
}
