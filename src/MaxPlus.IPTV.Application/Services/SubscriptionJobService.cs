using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class SubscriptionJobService : ISubscriptionJobService
{
    private readonly ICustomerSubscriptionRepository _subscriptionRepo;
    private readonly IWhatsAppService                _whatsApp;
    private readonly ISystemLogRepository            _logRepo;

    public SubscriptionJobService(
        ICustomerSubscriptionRepository subscriptionRepo,
        IWhatsAppService                whatsApp,
        ISystemLogRepository            logRepo)
    {
        _subscriptionRepo = subscriptionRepo;
        _whatsApp         = whatsApp;
        _logRepo          = logRepo;
    }

    public async Task SendExpirationNotificationsAsync(CancellationToken ct = default)
    {
        await NotifyExpiringAsync(7, ct);
        await NotifyExpiringAsync(1, ct);
    }

    public async Task AutoExpireSubscriptionsAsync(CancellationToken ct = default)
    {
        try
        {
            var cantidad = await _subscriptionRepo.ExpireOldAsync();
            await _logRepo.LogAsync(
                "Info",
                "SubscriptionJob",
                $"Auto-expiración completada. Suscripciones expiradas: {cantidad}.");
        }
        catch (Exception ex)
        {
            await _logRepo.LogAsync(
                "Error",
                "SubscriptionJob",
                "Error al auto-expirar suscripciones.",
                ex.ToString());
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task NotifyExpiringAsync(int daysAhead, CancellationToken ct)
    {
        IEnumerable<Core.Entities.CustomerSubscription> expiring;
        try
        {
            expiring = await _subscriptionRepo.GetExpiringAsync(daysAhead);
        }
        catch (Exception ex)
        {
            await _logRepo.LogAsync(
                "Error",
                "SubscriptionJob",
                $"Error al obtener suscripciones por vencer en {daysAhead} días.",
                ex.ToString());
            return;
        }

        foreach (var sub in expiring)
        {
            if (ct.IsCancellationRequested) break;

            if (string.IsNullOrWhiteSpace(sub.CustomerPhone))
            {
                await _logRepo.LogAsync(
                    "Warning",
                    "SubscriptionJob",
                    $"Suscripción {sub.Id}: cliente sin teléfono, notificación omitida.");
                continue;
            }

            try
            {
                await _whatsApp.SendExpirationReminderAsync(
                    sub.CustomerPhone,
                    sub.CustomerName ?? "Cliente",
                    sub.AccessUser ?? "",
                    sub.AccessPassword ?? "",
                    daysAhead);

                await _subscriptionRepo.MarkNotifiedAsync(sub.Id, daysAhead);
                await _logRepo.LogAsync(
                    "Info",
                    "SubscriptionJob",
                    $"Notificación WhatsApp enviada a {sub.CustomerPhone} — suscripción {sub.Id} vence en {daysAhead} día(s).");
            }
            catch (Exception ex)
            {
                await _logRepo.LogAsync(
                    "Error",
                    "SubscriptionJob",
                    $"Error al notificar suscripción {sub.Id} ({sub.CustomerPhone}).",
                    ex.ToString());
            }

            // Delay aleatorio entre mensajes para evitar baneo de WhatsApp
            var delayMs = Random.Shared.Next(4000, 10000);
            await Task.Delay(delayMs, ct);
        }
    }

    private static TimeZoneInfo GetNicaraguaTz()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("America/Managua"); }
    }
}
