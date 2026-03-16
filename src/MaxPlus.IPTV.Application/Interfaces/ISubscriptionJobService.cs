namespace MaxPlus.IPTV.Application.Interfaces;

public interface ISubscriptionJobService
{
    /// <summary>Notifica clientes con suscripciones que vencen en 7 días y 1 día.</summary>
    Task SendExpirationNotificationsAsync(CancellationToken ct = default);

    /// <summary>Marca como Expired las suscripciones activas ya vencidas.</summary>
    Task AutoExpireSubscriptionsAsync(CancellationToken ct = default);
}
