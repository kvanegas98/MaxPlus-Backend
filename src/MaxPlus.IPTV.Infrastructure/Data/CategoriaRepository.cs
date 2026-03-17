using Dapper;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly string _connectionString;

    public CategoriaRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<Categoria>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Categoria>("sp_Categorias_ObtenerTodos", commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Categoria>> GetActivasAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Categoria>("sp_Categorias_ObtenerActivas", commandType: CommandType.StoredProcedure);
    }

    public async Task<Categoria?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Categoria>(
            "sp_Categorias_ObtenerPorId",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> CreateAsync(Categoria categoria)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Nombre",      categoria.Nombre);
        parameters.Add("@Descripcion", categoria.Descripcion);
        parameters.Add("@Color",       categoria.Color);
        parameters.Add("@Orden",       categoria.Orden);
        parameters.Add("@Id",          dbType: DbType.Guid, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_Categorias_Crear", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("@Id");
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("sp_Categorias_Actualizar", new
        {
            categoria.Id,
            categoria.Nombre,
            categoria.Descripcion,
            categoria.Color,
            categoria.Orden,
            categoria.IsActive
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("sp_Categorias_Eliminar", new { Id = id }, commandType: CommandType.StoredProcedure);
    }
}
