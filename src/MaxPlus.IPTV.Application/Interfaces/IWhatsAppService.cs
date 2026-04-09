namespace MaxPlus.IPTV.Application.Interfaces;

public interface IWhatsAppService
{
    /// <param name="nombreAmigable">Nombre visible del servicio, ej: "Netflix", "Flujo TV", "IPTV"</param>
    /// <param name="labelUsuario">"Usuario" o "Correo" según la plataforma</param>
    /// <param name="tienePin">Si true se incluye el PIN de cuenta en el mensaje</param>
    /// <param name="profileUser">Usuario de perfil (Netflix/Streaming). Se agrega como línea extra si viene.</param>
    /// <param name="profilePin">PIN de perfil (Netflix/Streaming). Se agrega como línea extra si viene.</param>
    /// <param name="invoiceImageUrl">URL pública de la imagen PNG de la factura. Si se provee se envía adjunta al mismo mensaje.</param>
    Task SendCredentialsAsync(string toPhone, string customerName, string accessUser,
        string accessPassword, string? pinCode, DateTime expirationDate,
        string nombreAmigable, string labelUsuario, bool tienePin,
        string? profileUser = null, string? profilePin = null, string? invoiceImageUrl = null);

    Task SendDemoCredentialsAsync(string toPhone, string customerName,
        string accessUser, string accessPassword, string platformUrl);

    Task SendExpirationReminderAsync(string toPhone, string customerName,
        string accessUser, string accessPassword, int daysRemaining);

    Task SendPendingServicesAsync(string toPhone, string customerName,
        IEnumerable<string> pendingServices);
}
