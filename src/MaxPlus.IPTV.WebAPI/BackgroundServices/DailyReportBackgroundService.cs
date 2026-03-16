using MaxPlus.IPTV.Application.Interfaces;

namespace MaxPlus.IPTV.WebAPI.BackgroundServices;

/// <summary>
/// Ejecuta el reporte diario de ventas todos los días a las 7:00 PM hora Nicaragua (UTC-6).
/// </summary>
public class DailyReportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyReportBackgroundService> _logger;

    // 19:00 = 7 PM
    private static readonly TimeOnly TargetTime = new(19, 0, 0);

    public DailyReportBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyReportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "DailyReportBackgroundService iniciado. Próxima ejecución: {Next}",
            GetNextRunNicaragua().ToString("dd/MM/yyyy HH:mm:ss"));

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            if (stoppingToken.IsCancellationRequested) break;

            await RunReportAsync(stoppingToken);

            // Pausa de 2 minutos para no re-dispararse en el mismo minuto
            try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); }
            catch (TaskCanceledException) { break; }
        }

        _logger.LogInformation("DailyReportBackgroundService detenido.");
    }

    private async Task RunReportAsync(CancellationToken ct)
    {
        _logger.LogInformation("Ejecutando reporte diario de ventas...");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<IDailyReportJob>();
            await job.SendDailyReportAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar el reporte diario de ventas.");
        }
    }

    private static TimeSpan GetDelayUntilNextRun()
    {
        var tz         = GetNicaraguaTimeZone();
        var nextRunNic = GetNextRunNicaragua();
        // Convertir correctamente desde Nicaragua → UTC (evita que .ToUniversalTime()
        // asuma hora local del servidor en vez de hora Nicaragua)
        var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(nextRunNic, DateTimeKind.Unspecified), tz);
        var delay = nextRunUtc - DateTime.UtcNow;
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }

    private static DateTime GetNextRunNicaragua()
    {
        var tz     = GetNicaraguaTimeZone();
        var nowNic = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var todayTarget = nowNic.Date + TargetTime.ToTimeSpan();

        return nowNic < todayTarget ? todayTarget : todayTarget.AddDays(1);
    }

    private static TimeZoneInfo GetNicaraguaTimeZone()
    {
        // "Central America Standard Time" en Windows, "America/Managua" en Linux
        try { return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("America/Managua"); }
    }
}
