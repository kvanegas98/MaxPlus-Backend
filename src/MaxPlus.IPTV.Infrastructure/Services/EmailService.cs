using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MaxPlus.IPTV.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(IEnumerable<string> toEmails, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        foreach (var email in toEmails)
            message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var secureOption = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
        await client.ConnectAsync(_settings.Host, _settings.Port, secureOption);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private async Task SendWithBccAsync(string toEmail, IEnumerable<string>? bccEmails, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        if (bccEmails != null)
            foreach (var bcc in bccEmails)
                if (!string.IsNullOrWhiteSpace(bcc))
                    message.Bcc.Add(MailboxAddress.Parse(bcc));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var secureOption = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
        await client.ConnectAsync(_settings.Host, _settings.Port, secureOption);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public Task SendCredentialsAsync(string toEmail, string customerName,
        string accessUser, string accessPassword, string? pinCode,
        DateTime expirationDate, string serviceName,
        string labelUsuario, string? platformUrl = null,
        string? profileUser = null, string? profilePin = null,
        bool isRenewal = false)
    {
        var titulo = isRenewal
            ? $"&#x2705; Tu servicio {serviceName} ha sido renovado"
            : $"&#x2705; Tu servicio {serviceName} está listo";

        var pinHtml = pinCode is not null
            ? $"<tr><td style='color:#aaa;padding:4px 0'>PIN</td><td style='color:#fff;font-weight:600'>{pinCode}</td></tr>"
            : string.Empty;

        var perfilHtml = (profileUser is not null || profilePin is not null)
            ? $"<tr><td colspan='2' style='padding:12px 0 4px;font-weight:600;color:#8B5CF6;'>Perfil asignado</td></tr>"
              + (profileUser is not null ? $"<tr><td style='color:#aaa;padding:4px 0'>Usuario de perfil</td><td style='color:#fff;font-weight:600'>{profileUser}</td></tr>" : "")
              + (profilePin is not null ? $"<tr><td style='color:#aaa;padding:4px 0'>PIN de perfil</td><td style='color:#fff;font-weight:600'>{profilePin}</td></tr>" : "")
            : string.Empty;

        var plataformaHtml = platformUrl is not null
            ? $"<p style='margin:16px 0 0'><a href='{platformUrl}' style='background:#8B5CF6;color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;font-weight:600'>Abrir plataforma</a></p>"
            : string.Empty;

        var html = $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#0D0B1E;font-family:Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
    <tr><td align=""center"" style=""padding:32px 16px;"">
      <table width=""480"" cellpadding=""0"" cellspacing=""0"" style=""background:#1A1535;border-radius:12px;overflow:hidden;"">
        <tr><td style=""background:#8B5CF6;padding:24px 32px;"">
          <h1 style=""margin:0;color:#fff;font-size:20px;"">MaxPlus IPTV</h1>
          <p style=""margin:6px 0 0;color:#e0d7ff;font-size:14px;"">{titulo}</p>
        </td></tr>
        <tr><td style=""padding:28px 32px;color:#fff;"">
          <p style=""margin:0 0 20px"">Hola <strong>{customerName}</strong>, aquí están tus credenciales de acceso:</p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:15px;"">
            <tr><td style=""color:#aaa;padding:4px 0"">{labelUsuario}</td><td style=""color:#fff;font-weight:600"">{accessUser}</td></tr>
            <tr><td style=""color:#aaa;padding:4px 0"">Contraseña</td><td style=""color:#fff;font-weight:600"">{accessPassword}</td></tr>
            {pinHtml}
            {perfilHtml}
            <tr><td style=""color:#aaa;padding:4px 0"">Vence</td><td style=""color:#10B981;font-weight:600"">{expirationDate:dd/MM/yyyy}</td></tr>
          </table>
          {plataformaHtml}
          <p style=""margin:24px 0 0;font-size:13px;color:#888;"">Guarda este correo. Si tienes problemas para acceder, contáctanos por WhatsApp.</p>
        </td></tr>
        <tr><td style=""background:#0D0B1E;padding:16px 32px;text-align:center;"">
          <p style=""margin:0;color:#555;font-size:12px;"">MaxPlus IPTV &bull; Este es un correo automático.</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        var subject = isRenewal
            ? $"Renovación de servicio: {serviceName}"
            : $"Credenciales de acceso: {serviceName}";

        return SendWithBccAsync(toEmail, _settings.ReportRecipients, subject, html);
    }

    public Task SendDemoCredentialsAsync(string toEmail, string customerName,
        string accessUser, string accessPassword, string platformUrl)
    {
        var html = $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#0D0B1E;font-family:Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
    <tr><td align=""center"" style=""padding:32px 16px;"">
      <table width=""480"" cellpadding=""0"" cellspacing=""0"" style=""background:#1A1535;border-radius:12px;overflow:hidden;"">
        <tr><td style=""background:#8B5CF6;padding:24px 32px;"">
          <h1 style=""margin:0;color:#fff;font-size:20px;"">MaxPlus IPTV</h1>
          <p style=""margin:6px 0 0;color:#e0d7ff;font-size:14px;"">🎬 Tu demo está lista</p>
        </td></tr>
        <tr><td style=""padding:28px 32px;color:#fff;"">
          <p style=""margin:0 0 20px"">Hola <strong>{customerName}</strong>, aquí están tus credenciales de prueba:</p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:15px;"">
            <tr><td style=""color:#aaa;padding:4px 0"">Usuario</td><td style=""color:#fff;font-weight:600"">{accessUser}</td></tr>
            <tr><td style=""color:#aaa;padding:4px 0"">Contraseña</td><td style=""color:#fff;font-weight:600"">{accessPassword}</td></tr>
            <tr><td style=""color:#aaa;padding:4px 0"">URL</td><td style=""color:#10B981;font-weight:600"">{platformUrl}</td></tr>
          </table>
          <p style=""margin:20px 0 0;font-size:13px;color:#888;"">Esta demo es válida por tiempo limitado. Si tienes problemas contáctanos por WhatsApp.</p>
        </td></tr>
        <tr><td style=""background:#0D0B1E;padding:16px 32px;text-align:center;"">
          <p style=""margin:0;color:#555;font-size:12px;"">MaxPlus IPTV &bull; Este es un correo automático.</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        return SendAsync([toEmail], "🎬 Tus credenciales de demo — MaxPlus IPTV", html);
    }

    public Task SendDemoNotificationAsync(IEnumerable<string> adminEmails,
        string customerName, string customerPhone, string? customerEmail, string? serviceName)
    {
        var html = $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#0D0B1E;font-family:Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
    <tr><td align=""center"" style=""padding:32px 16px;"">
      <table width=""480"" cellpadding=""0"" cellspacing=""0"" style=""background:#1A1535;border-radius:12px;overflow:hidden;"">
        <tr><td style=""background:#FF6B00;padding:24px 32px;"">
          <h1 style=""margin:0;color:#fff;font-size:20px;"">MaxPlus IPTV</h1>
          <p style=""margin:6px 0 0;color:#ffe0cc;font-size:14px;"">⚡ Nueva solicitud de demo</p>
        </td></tr>
        <tr><td style=""padding:28px 32px;color:#fff;"">
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:15px;"">
            <tr><td style=""color:#aaa;padding:4px 0"">Cliente</td><td style=""color:#fff;font-weight:600"">{customerName}</td></tr>
            <tr><td style=""color:#aaa;padding:4px 0"">Teléfono</td><td style=""color:#fff;font-weight:600"">{customerPhone}</td></tr>
            <tr><td style=""color:#aaa;padding:4px 0"">Correo</td><td style=""color:#fff;font-weight:600"">{customerEmail ?? "No proporcionado"}</td></tr>
            <tr><td style=""color:#aaa;padding:4px 0"">Servicio</td><td style=""color:#8B5CF6;font-weight:600"">{serviceName ?? "No especificado"}</td></tr>
          </table>
        </td></tr>
        <tr><td style=""background:#0D0B1E;padding:16px 32px;text-align:center;"">
          <p style=""margin:0;color:#555;font-size:12px;"">MaxPlus IPTV &bull; Notificación automática.</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        return SendAsync(adminEmails, "⚡ Nueva solicitud de demo — MaxPlus IPTV", html);
    }
}
