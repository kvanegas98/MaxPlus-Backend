using System.Net.Http;
using System.Text.Json;
using MaxPlus.IPTV.Application.Interfaces;

namespace MaxPlus.IPTV.Infrastructure.Services;

public class IptvPanelClientService : IIptvPanelClientService
{
    private readonly HttpClient _httpClient;

    public IptvPanelClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PanelCreatedAccountDto> CreateTrialAccountAsync(string hostUrl, string username, string password, string customerName)
    {
        hostUrl = hostUrl.TrimEnd('/');
        
        var rnd = new Random();
        string newUsername = $"demo{rnd.Next(100, 999)}_{rnd.Next(100, 999)}";
        string newPassword = rnd.Next(10000000, 99999999).ToString();
        
        try
        {
            // --- PASO 1: LOGIN Y CAPTURA DE COOKIE (PHPSESSID) ---
            
            // Creamos un handler interno para asegurarnos que maneja las cookies entre peticiones.
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
#pragma warning disable SYSLIB0039
                SslProtocols = System.Security.Authentication.SslProtocols.Tls
                             | System.Security.Authentication.SslProtocols.Tls11
                             | System.Security.Authentication.SslProtocols.Tls12
                             | System.Security.Authentication.SslProtocols.Tls13
#pragma warning restore SYSLIB0039
            };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(hostUrl) };
            
            // Añadimos las cabeceras estándar para parecer un navegador
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");

            // PASO 1a: GET a /login.php para obtener el PHPSESSID antes del POST
            await client.GetAsync("/login.php");

            // PASO 1b: POST de login con Referer y Origin requeridos por el panel
            var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/login_process.php")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                })
            };
            loginRequest.Headers.Add("Referer", $"{hostUrl}/login.php");
            loginRequest.Headers.Add("Origin", hostUrl);

            var loginResponse = await client.SendAsync(loginRequest);
            var loginHtml = await loginResponse.Content.ReadAsStringAsync();

            if (loginHtml.Contains("blocked", StringComparison.OrdinalIgnoreCase) ||
                loginResponse.RequestMessage?.RequestUri?.AbsolutePath.Contains("login") == true)
            {
                return new PanelCreatedAccountDto
                {
                    IsSuccess = false,
                    Message = $"Login fallido. URL final: {loginResponse.RequestMessage?.RequestUri} | HTML: {loginHtml[..Math.Min(400, loginHtml.Length)]}"
                };
            }

            // --- PASO 2: CREACIÓN DEL USUARIO ---
            
            // En este punto el 'client' ya atrapó internamente el PHPSESSID en sus cookies.
            var subContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("cliente_nombre", customerName),
                new KeyValuePair<string, string>("cliente_whatsapp", "00000000"),
                new KeyValuePair<string, string>("tipo_servicio", "demo"),
                new KeyValuePair<string, string>("package_id", "46"), // Id Fijo de paquete según tu cURL
                new KeyValuePair<string, string>("username_new", newUsername),
                new KeyValuePair<string, string>("password_new", newPassword),
                new KeyValuePair<string, string>("bouquets[]", "30"),
                new KeyValuePair<string, string>("bouquets[]", "5"),
                new KeyValuePair<string, string>("bouquets[]", "15"),
                new KeyValuePair<string, string>("bouquets[]", "16"),
                new KeyValuePair<string, string>("bouquets[]", "9"),
                new KeyValuePair<string, string>("bouquets[]", "6"),
                new KeyValuePair<string, string>("bouquets[]", "22"),
                new KeyValuePair<string, string>("bouquets[]", "27"),
                new KeyValuePair<string, string>("notes_new", "MaxPlus API AutoDemo"),
                new KeyValuePair<string, string>("create_line", "") 
            });

            var subResponse = await client.PostAsync("/suscripciones.php", subContent);
            var resultHtml = await subResponse.Content.ReadAsStringAsync();

            if (subResponse.IsSuccessStatusCode)
            {
                // Validación básica de Scraping (Depende del panel, usualmente tiran un cartel verde de 'Suscripción agregada')
                if (resultHtml.Contains("exitosamente", StringComparison.OrdinalIgnoreCase) || 
                    resultHtml.Contains("agregada", StringComparison.OrdinalIgnoreCase) ||
                    resultHtml.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    return new PanelCreatedAccountDto
                    {
                        Username = newUsername,
                        Password = newPassword,
                        IsSuccess = true,
                        Message = "Demo generado exitosamente vía Web Scraping."
                    };
                }
                
                // Si el panel devuelve error (ej. Usuario ya existe, Sin creditos)
                return new PanelCreatedAccountDto
                {
                    IsSuccess = false,
                    Message = $"Panel rechazó la creación. HTML recibido: {resultHtml[..Math.Min(800, resultHtml.Length)]}"
                };
            }

            return new PanelCreatedAccountDto
            {
                IsSuccess = false,
                Message = $"Error HTTP al crear suscripción: {subResponse.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new PanelCreatedAccountDto
            {
                IsSuccess = false,
                Message = $"Error: {ex.Message} | Inner: {ex.InnerException?.Message} | Inner2: {ex.InnerException?.InnerException?.Message}"
            };
        }
    }
}
