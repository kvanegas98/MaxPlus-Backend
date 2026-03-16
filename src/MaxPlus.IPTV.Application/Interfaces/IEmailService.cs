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
}
