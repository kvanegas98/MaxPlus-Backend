using System.Data;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class CustomerRepository : ICustomerRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CustomerRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(
            "sp_Clientes_ObtenerTodos",
            commandType: CommandType.StoredProcedure);

        return rows.Select(MapToCustomer);
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);

        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "sp_Clientes_ObtenerPorId",
            parameters,
            commandType: CommandType.StoredProcedure);

        return row is null ? null : MapToCustomer(row);
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string term)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Termino", term);

        var rows = await connection.QueryAsync<dynamic>(
            "sp_Clientes_Buscar",
            parameters,
            commandType: CommandType.StoredProcedure);

        return rows.Select(MapToCustomer);
    }

    public async Task<Customer?> FindByContactAsync(string? email, string? phone)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Email",  email);
        parameters.Add("@Phone",  phone);

        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "sp_Clientes_BuscarPorContacto",
            parameters,
            commandType: CommandType.StoredProcedure);

        return row is null ? null : MapToCustomer(row);
    }

    public async Task<Guid> AddAsync(Customer customer)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Nombre",    customer.Name);
        parameters.Add("@Telefono",  customer.Phone);
        parameters.Add("@Direccion", customer.Address);
        parameters.Add("@Email",     customer.Email);
        parameters.Add("@Id",        dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_Clientes_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task UpdateAsync(Customer customer)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id",        customer.Id);
        parameters.Add("@Nombre",    customer.Name);
        parameters.Add("@Telefono",  customer.Phone);
        parameters.Add("@Direccion", customer.Address);
        parameters.Add("@Email",     customer.Email);

        await connection.ExecuteAsync(
            "sp_Clientes_Actualizar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);

        await connection.ExecuteAsync(
            "sp_Clientes_Desactivar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    private static Customer MapToCustomer(dynamic r) => new()
    {
        Id             = (Guid)r.Id,
        Name           = (string)r.Nombre,
        Phone          = (string?)r.Telefono,
        Address        = (string?)r.Direccion,
        Email          = (string?)r.Email,
        IsActive       = r.IsActive is bool b ? b : true,
        CreatedAt      = (DateTime)r.CreatedAt
    };
}
