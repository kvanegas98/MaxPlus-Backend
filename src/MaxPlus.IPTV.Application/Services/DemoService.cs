using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Helpers;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MaxPlus.IPTV.Application.Services;

public class DemoService : IDemoService
{
    private readonly IDemoRequestRepository  _repository;
    private readonly IIptvPanelClientService _panelClient;
    private readonly IWhatsAppService        _whatsApp;
    private readonly IConfiguration          _config;

    public DemoService(IDemoRequestRepository  repository,
                       IIptvPanelClientService panelClient,
                       IWhatsAppService        whatsApp,
                       IConfiguration          config)
    {
        _repository  = repository;
        _panelClient = panelClient;
        _whatsApp    = whatsApp;
        _config      = config;
    }

    public async Task<DemoRequestResponseDto> RequestDemoAsync(DemoRequestCreateDto dto, string? ipAddress, string? country)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerPhone))
            throw new InvalidOperationException("El teléfono del cliente es requerido para enviar las credenciales por WhatsApp.");

        // 1. Crear la solicitud en BD
        var request = new DemoRequest
        {
            CustomerName   = dto.CustomerName.Trim(),
            CustomerPhone  = dto.CustomerPhone.Trim(),
            CustomerEmail  = dto.CustomerEmail?.Trim(),
            CustomerId     = dto.CustomerId,
            TipoServicioId = dto.TipoServicioId,
            IpAddress      = ipAddress,
            Country        = country,
            IsPhoneVerified = true
        };

        request.Id = await _repository.AddAsync(request);

        // 2. Crear cuenta trial en el panel del proveedor
        var panelHost = _config["XtreamPanel:HostUrl"];
        var panelUser = _config["XtreamPanel:Username"];
        var panelPass = _config["XtreamPanel:Password"];

        if (string.IsNullOrEmpty(panelHost) || string.IsNullOrEmpty(panelUser) || string.IsNullOrEmpty(panelPass))
            throw new Exception("Las credenciales del proveedor IPTV no están configuradas en appsettings.json.");

        var panelResponse = await _panelClient.CreateTrialAccountAsync(panelHost, panelUser, panelPass, request.CustomerName);

        if (!panelResponse.IsSuccess)
            throw new Exception(panelResponse.Message);

        // 3. Auto-aprobar y guardar credenciales
        string htmlRes = $"<ul><li><strong>Usuario:</strong> {panelResponse.Username}</li><li><strong>Contraseña:</strong> {panelResponse.Password}</li><li><strong>URL:</strong> {panelHost}</li></ul>";
        await _repository.ApproveAsync(request.Id, null, panelHost, htmlRes);

        // 4. Enviar credenciales por WhatsApp
        try
        {
            await _whatsApp.SendDemoCredentialsAsync(
                request.CustomerPhone, request.CustomerName,
                panelResponse.Username, panelResponse.Password, panelHost);
        }
        catch { /* No bloquear si falla WhatsApp */ }

        var updated = await _repository.GetByIdAsync(request.Id);
        return updated is null ? MapToDto(request) : MapToDto(updated);
    }

    public async Task<DemoRequestResponseDto?> VerifyPhoneAsync(Guid id, string code)
    {
        // 1. Verificar expiración del OTP (10 minutos desde la solicitud)
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Demo Request no encontrado.");

        if (DateTimeHelper.GetNicaraguaTime() > existing.CreatedAt.ToNicaraguaTime().AddMinutes(10))
            throw new InvalidOperationException("El código de verificación ha expirado. Solicita un nuevo demo.");

        // 2. Verificamos el código contra la base de datos
        await _repository.VerifyPhoneAsync(id, code);

        // 2. Traer el request original para ver de quién es
        var request = await _repository.GetByIdAsync(id) 
            ?? throw new KeyNotFoundException("Demo Request no encontrado al verificar.");

        if (request.Status != "Pending")
            throw new InvalidOperationException("Este Demo ya fue procesado o no está pendiente.");

        // 3. Traer la cuenta maestra del Proveedor (Desde appsettings.json)
        var panelHost = _config["XtreamPanel:HostUrl"];
        var panelUser = _config["XtreamPanel:Username"];
        var panelPass = _config["XtreamPanel:Password"];

        if (string.IsNullOrEmpty(panelHost) || string.IsNullOrEmpty(panelUser) || string.IsNullOrEmpty(panelPass))
            throw new Exception("Las credenciales del proveedor IPTV no están configuradas en appsettings.json.");

        // 4. Conectarnos al panel por HTTP
        var panelResponse = await _panelClient.CreateTrialAccountAsync(
            panelHost, 
            panelUser, 
            panelPass, 
            request.CustomerName
        );

        if (!panelResponse.IsSuccess)
        {
            throw new Exception(panelResponse.Message);
        }

        // 5. Construir la respuesta para guardar y mostrar
        string urlDemo = panelHost;
        string htmlRes = $@"
            <h4>Demo Generado Exitosamente</h4>
            <p><strong>App recomendada:</strong> Smarters Player Lite / TiviMate</p>
            <ul>
                <li><strong>Usuario:</strong> {panelResponse.Username}</li>
                <li><strong>Contraseña:</strong> {panelResponse.Password}</li>
                <li><strong>URL:</strong> {urlDemo}</li>
            </ul>
        ";

        // 6. Auto-aprobar internamente el request (El ID de aprobado puede ser Guid.Empty simulando al sistema)
        await _repository.ApproveAsync(id, null, urlDemo, htmlRes);

        // 7. Retornar el objeto actualizado para que el controlador lo entregue al usuario
        var updatedRequest = await _repository.GetByIdAsync(id);
        return updatedRequest is null ? null : MapToDto(updatedRequest);
    }

    public async Task<IEnumerable<DemoRequestResponseDto>> GetAllAsync(string? status = null)
    {
        var requests = await _repository.GetAllAsync(status);
        return requests.Select(MapToDto);
    }

    public async Task<DemoRequestResponseDto?> GetByIdAsync(Guid id)
    {
        var request = await _repository.GetByIdAsync(id);
        return request is null ? null : MapToDto(request);
    }

    public async Task<DemoRequestResponseDto> ApproveAsync(Guid id, Guid approvedBy, DemoApproveDto dto)
    {
        await _repository.ApproveAsync(id, approvedBy, dto.DemoUrl, dto.ResponseHtml);

        var updated = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Demo request con ID {id} no encontrada.");

        return MapToDto(updated);
    }

    public async Task RejectAsync(Guid id, Guid approvedBy, DemoRejectDto dto)
    {
        await _repository.RejectAsync(id, approvedBy, dto.Reason);
    }

    private static DemoRequestResponseDto MapToDto(DemoRequest r) => new()
    {
        Id              = r.Id,
        CustomerName    = r.CustomerName,
        CustomerPhone   = r.CustomerPhone,
        CustomerEmail   = r.CustomerEmail,
        CustomerId      = r.CustomerId,
        TipoServicioId  = r.TipoServicioId,
        IpAddress       = r.IpAddress,
        Country         = r.Country,
        IsPhoneVerified = r.IsPhoneVerified,
        ServiceName     = r.ServiceName,
        Status          = r.Status,
        DemoUrl         = r.DemoUrl,
        ResponseHtml    = r.ResponseHtml,
        ApprovedBy      = r.ApprovedBy,
        ApprovedAt      = r.ApprovedAt,
        ExpiresAt       = r.ExpiresAt,
        RejectionReason = r.RejectionReason,
        CreatedAt       = r.CreatedAt
    };
}
