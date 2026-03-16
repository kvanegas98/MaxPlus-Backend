using MaxPlus.IPTV.Application.Interfaces;

namespace MaxPlus.IPTV.WebAPI.BackgroundServices;

/// <summary>
/// Ejecuta los jobs de suscripciones todos los días a las 9:00 AM hora Nicaragua (UTC-6).
/// • Notifica clientes con vencimiento en 7 y 1 día.
/// • Expira automáticamente las suscripciones vencidas.
/// </summary>
public class SubscriptionJobBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory                   _scopeFactory;
    private readonly ILogger<SubscriptionJobBackgroundService> _logger;

    // 9:00 AM Nicaragua
    private static readonly TimeOnly TargetTime = new(9, 0, 0);

    public SubscriptionJobBackgroundService(
        IServiceScopeFactory                   scopeFactory,
        ILogger<SubscriptionJobBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SubscriptionJobBackgroundService iniciado. Próxima ejecución: {Next}",
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

            await RunJobsAsync(stoppingToken);

            // Pausa de 2 minutos para no re-dispararse en el mismo minuto
            try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); }
            catch (TaskCanceledException) { break; }
        }

        _logger.LogInformation("SubscriptionJobBackgroundService detenido.");
    }

    private async Task RunJobsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Ejecutando jobs de suscripciones...");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<ISubscriptionJobService>();
            await job.SendExpirationNotificationsAsync(ct);
            await job.AutoExpireSubscriptionsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar los jobs de suscripciones.");
        }
    }

    private static TimeSpan GetDelayUntilNextRun()
    {
        var tz         = GetNicaraguaTimeZone();
        var nextRunNic = GetNextRunNicaragua();
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
        try { return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("America/Managua"); }
    }
}
