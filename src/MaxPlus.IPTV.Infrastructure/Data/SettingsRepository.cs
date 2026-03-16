using System.Data;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Dapper;

namespace MaxPlus.IPTV.Infrastructure.Data;

public class SettingsRepository : ISettingsRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SettingsRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Settings?> GetAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "sp_Configuracion_Obtener",
            commandType: CommandType.StoredProcedure);

        return row is null ? null : MapToSettings(row);
    }

    public async Task UpdateAsync(Settings settings)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@NombreNegocio",         settings.BusinessName);
        parameters.Add("@Telefono",              settings.Phone);
        parameters.Add("@Descripcion",           settings.Description);
        parameters.Add("@Direccion",             settings.Address);
        parameters.Add("@LogoUrl",               settings.LogoUrl);
        parameters.Add("@TasaCambioUSD",         settings.ExchangeRateUSD);
        parameters.Add("@DemoPhpBaseUrl",        settings.DemoPhpBaseUrl);
        parameters.Add("@MenuPublicoHabilitado", settings.PublicMenuEnabled);
        parameters.Add("@DemoAutoApprove",       settings.DemoAutoApprove);

        await connection.ExecuteAsync(
            "sp_Configuracion_Actualizar",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    private static Settings MapToSettings(dynamic r) => new()
    {
        Id                    = (int)r.Id,
        BusinessName          = (string)r.NombreNegocio,
        Phone                 = (string?)r.Telefono,
        Description           = (string?)r.Descripcion,
        Address               = (string?)r.Direccion,
        LogoUrl               = (string?)r.LogoUrl,
        ExchangeRateUSD       = (decimal)r.TasaCambioUSD,
        DemoPhpBaseUrl        = (string?)r.DemoPhpBaseUrl,
        PublicMenuEnabled     = (bool)r.MenuPublicoHabilitado,
        DemoAutoApprove       = (bool)r.DemoAutoApprove,
        UpdatedAt             = (DateTime?)r.UpdatedAt
    };
}
