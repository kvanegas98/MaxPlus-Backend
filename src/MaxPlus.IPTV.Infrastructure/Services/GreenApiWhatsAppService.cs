using System.Text;
using System.Text.Json;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MaxPlus.IPTV.Infrastructure.Services;

public class GreenApiWhatsAppService : IWhatsAppService
{
    private static readonly HttpClient _http = new();
    private readonly string _baseUrl;
    private readonly string _apiToken;

    public GreenApiWhatsAppService(IConfiguration config)
    {
        var instanceId = config["GreenApi:InstanceId"]!;
        _apiToken      = config["GreenApi:ApiToken"]!;
        _baseUrl       = $"https://api.green-api.com/waInstance{instanceId}";
    }

    // ─────────────────────────────────────────────
    // Credenciales + factura (imagen adjunta con caption)
    // ─────────────────────────────────────────────
    public async Task SendCredentialsAsync(
        string toPhone, string customerName,
        string accessUser, string accessPassword, string? pinCode, DateTime expirationDate,
        string nombreAmigable, string labelUsuario, bool tienePin,
        string? profileUser = null, string? profilePin = null, string? invoiceImageUrl = null)
    {
        var creds = new StringBuilder();
        creds.AppendLine($"• {labelUsuario}: {accessUser}");
        creds.AppendLine($"• Contraseña: {accessPassword}");
        if (tienePin && pinCode != null)
            creds.AppendLine($"• PIN: {pinCode}");
        if (profileUser != null)
            creds.AppendLine($"• Usuario de perfil: {profileUser}");
        if (profilePin != null)
            creds.AppendLine($"• PIN de perfil: {profilePin}");

        var mensaje = $"""
            Hola {customerName} 👋

            ✅ *Tu servicio {nombreAmigable} está listo*

            📋 *Credenciales:*
            {creds.ToString().TrimEnd()}

            📅 Vence: {expirationDate:dd/MM/yyyy}

            🧾 *Adjunto encontrarás tu comprobante de pago.*

            _MaxPlus IPTV_
            """.Trim();

        var chatId = FormatChatId(toPhone);

        if (invoiceImageUrl is not null)
        {
            // Enviar imagen con el texto como caption
            await SendFileByUrlAsync(chatId, invoiceImageUrl, "factura.png", mensaje);
        }
        else
        {
            await SendMessageAsync(chatId, mensaje);
        }
    }

    // ─────────────────────────────────────────────
    // Demo
    // ─────────────────────────────────────────────
    public async Task SendDemoCredentialsAsync(string toPhone, string customerName,
        string accessUser, string accessPassword, string platformUrl)
    {
        var mensaje = $"""
            🎬 *PRUEBA GRATIS IPTV*

            Hola {customerName}, para ingresar al demo debes colocar los datos exactamente como se muestran, respetando mayúsculas, minúsculas y caracteres.

            👤 *Usuario:* {accessUser}
            🔐 *Clave:* {accessPassword}

            ⏱ Válida por 4 horas.

            _MaxPlus IPTV_
            """.Trim();

        await SendMessageAsync(FormatChatId(toPhone), mensaje);
    }

    // ─────────────────────────────────────────────
    // Recordatorio de vencimiento
    // ─────────────────────────────────────────────
    public async Task SendExpirationReminderAsync(string toPhone, string customerName,
        string accessUser, string accessPassword, int daysRemaining)
    {
        var mensaje = $"""
            Hola {customerName} 👋

            ⚠️ *Tu servicio vence en {daysRemaining} días*

            Para renovar comunícate con nosotros.

            📋 *Tus credenciales actuales:*
            • Usuario: {accessUser}
            • Contraseña: {accessPassword}

            _MaxPlus IPTV_
            """.Trim();

        await SendMessageAsync(FormatChatId(toPhone), mensaje);
    }

    // ─────────────────────────────────────────────
    // Servicios pendientes (aprobación parcial)
    // ─────────────────────────────────────────────
    public async Task SendPendingServicesAsync(string toPhone, string customerName,
        IEnumerable<string> pendingServices)
    {
        var lista = string.Join("\n", pendingServices.Select(s => $"• {s}"));

        var mensaje = $"""
            Hola {customerName} 👋

            ✅ Hemos procesado parte de tu pedido.

            ⏳ *Los siguientes servicios están en espera de disponibilidad:*
            {lista}

            Te contactaremos en cuanto estén listos. ¡Gracias por tu paciencia!

            _MaxPlus IPTV_
            """.Trim();

        await SendMessageAsync(FormatChatId(toPhone), mensaje);
    }

    // ─────────────────────────────────────────────
    // Helpers internos
    // ─────────────────────────────────────────────
    private async Task SendMessageAsync(string chatId, string message)
    {
        var url     = $"{_baseUrl}/sendMessage/{_apiToken}";
        var payload = JsonSerializer.Serialize(new { chatId, message });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    private async Task SendFileByUrlAsync(string chatId, string urlFile, string fileName, string caption)
    {
        var url     = $"{_baseUrl}/sendFileByUrl/{_apiToken}";
        var payload = JsonSerializer.Serialize(new { chatId, urlFile, fileName, caption });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    // Convierte teléfono a formato Green API: 50588001234@c.us
    private static string FormatChatId(string phone)
    {
        phone = phone.Trim().Replace(" ", "").Replace("-", "");
        if (phone.StartsWith("+"))
            phone = phone[1..];
        else if (!phone.StartsWith("505"))
            phone = "505" + phone;

        return $"{phone}@c.us";
    }
}
