namespace MaxPlus.IPTV.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(IEnumerable<string> toEmails, string subject, string htmlBody);

    /// <summary>
    /// Envía las credenciales del servicio al cliente como respaldo del WhatsApp.
    /// </summary>
    Task SendCredentialsAsync(string toEmail, string customerName,
        string accessUser, string accessPassword, string? pinCode,
        DateTime expirationDate, string serviceName,
        string labelUsuario, string? platformUrl = null,
        string? profileUser = null, string? profilePin = null,
        bool isRenewal = false);

    // Envía credenciales de demo al cliente
    Task SendDemoCredentialsAsync(string toEmail, string customerName,
        string accessUser, string accessPassword, string platformUrl);

    // Notifica a los admins que llegó una nueva solicitud de demo
    Task SendDemoNotificationAsync(IEnumerable<string> adminEmails,
        string customerName, string customerPhone, string? customerEmail, string? serviceName);
}
