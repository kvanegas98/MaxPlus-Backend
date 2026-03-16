using System.Data;
using Dapper;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class PlataformaConfigRepository : IPlataformaConfigRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public PlataformaConfigRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<PlataformaConfig>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PlataformaConfig>(
            "sp_PlataformasConfig_ObtenerTodas",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PlataformaConfig?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PlataformaConfig>(
            "sp_PlataformasConfig_ObtenerPorId",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(PlataformaConfig config)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Plataforma",     config.Plataforma);
        parameters.Add("@NombreAmigable", config.NombreAmigable);
        parameters.Add("@LabelUsuario",   config.LabelUsuario);
        parameters.Add("@TieneUrl",       config.TieneUrl);
        parameters.Add("@TienePin",       config.TienePin);
        parameters.Add("@TieneCorreo",    config.TieneCorreo);
        parameters.Add("@NewId",          dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_PlataformasConfig_Crear",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task UpdateAsync(PlataformaConfig config)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_PlataformasConfig_Actualizar",
            new
            {
                config.Id,
                config.Plataforma,
                config.NombreAmigable,
                config.LabelUsuario,
                config.TieneUrl,
                config.TienePin,
                config.TieneCorreo
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeactivateAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "sp_PlataformasConfig_Desactivar",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }
}
