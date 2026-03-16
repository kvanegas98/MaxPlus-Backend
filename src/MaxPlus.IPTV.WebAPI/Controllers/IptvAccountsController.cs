using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace MaxPlus.IPTV.WebAPI.Controllers;

/// <summary>
/// Gestión de cuentas IPTV.
/// Una cuenta IPTV tiene credenciales compartidas y puede asignarse a N clientes (slots).
/// </summary>
[ApiController]
[Route("api/iptv-accounts")]
[Authorize]
[EnableRateLimiting("fixed-general")]
public class IptvAccountsController : ControllerBase
{
    private readonly IIptvAccountService _service;

    public IptvAccountsController(IIptvAccountService service)
    {
        _service = service;
    }

    // ── Vista principal: cuentas con sus clientes y días restantes ─────────

    /// <summary>
    /// Vista paginada: cuentas IPTV con sus clientes y días restantes.
    /// Ordenado de mayor a menor por fecha de creación.
    /// </summary>
    [HttpGet("view")]
    public async Task<IActionResult> GetAllWithClients(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page     < 1)  page     = 1;
        if (pageSize < 1)  pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var result = await _service.GetAllWithClientsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Totales para el dashboard: cuentas activas, slots totales, ocupados y libres.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lista cuentas IPTV activas de un tipo de servicio específico, con slots disponibles.
    /// Útil para seleccionar la cuenta al aprobar una orden desde el frontend.
    /// </summary>
    [HttpGet("by-service/{tipoServicioId:guid}")]
    public async Task<IActionResult> GetByServiceType(Guid tipoServicioId)
    {
        var result = await _service.GetByServiceTypeAsync(tipoServicioId);
        return Ok(result);
    }

    // ── CRUD Cuentas ───────────────────────────────────────────────────────

    /// <summary>Lista todas las cuentas IPTV (sin detalle de clientes).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Obtiene una cuenta con todos sus clientes.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetWithClientsAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Crea una cuenta IPTV (las credenciales que compraste al proveedor).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IptvAccountCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza credenciales y configuración de la cuenta.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] IptvAccountUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Desactiva la cuenta (no elimina, conserva historial).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _service.DeactivateAsync(id);
        return NoContent();
    }

    // ── Asignar cliente a un slot ──────────────────────────────────────────

    /// <summary>
    /// Asigna un cliente a un slot disponible de la cuenta.
    /// Si no existe el cliente, lo crea. Si se envía pago, genera factura.
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignClient(Guid id, [FromBody] IptvAccountAssignClientDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.AssignClientAsync(id, dto, userId);
        return Ok(result);
    }
}
