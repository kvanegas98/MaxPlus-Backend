using System.Data;
using Dapper;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class ServiceTypeRepository : IServiceTypeRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ServiceTypeRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ServiceType>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ServiceType>(
            "sp_TiposServicio_ObtenerTodos",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<ServiceType?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ServiceType>(
            "sp_TiposServicio_ObtenerPorId",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> CreateAsync(ServiceType serviceType)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Nombre",       serviceType.Name);
        parameters.Add("@Descripcion",  serviceType.Description);
        parameters.Add("@Precio",       serviceType.Price);
        parameters.Add("@PrecioCompra", serviceType.PurchasePrice);
        parameters.Add("@DurationDays", serviceType.DurationDays);
        parameters.Add("@Category",     serviceType.Category);
        parameters.Add("@Plataforma",   serviceType.Plataforma);
        parameters.Add("@ImageUrl",     serviceType.ImageUrl);
        parameters.Add("@Id",           dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_TiposServicio_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<Guid>("@Id");
    }

    public async Task UpdateAsync(ServiceType serviceType)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_TiposServicio_Actualizar",
            new
            {
                Id           = serviceType.Id,
                Nombre       = serviceType.Name,
                Descripcion  = serviceType.Description,
                Precio       = serviceType.Price,
                PrecioCompra = serviceType.PurchasePrice,
                DurationDays = serviceType.DurationDays,
                Category     = serviceType.Category,
                Plataforma   = serviceType.Plataforma,
                ImageUrl     = serviceType.ImageUrl
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeactivateAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_TiposServicio_Desactivar",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }
}
