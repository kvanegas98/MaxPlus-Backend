using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ICustomerSubscriptionService _service;

    public SubscriptionsController(ICustomerSubscriptionService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : throw new UnauthorizedAccessException("Token inválido.");
    }

    /// <summary>Obtiene las suscripciones de un cliente.</summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<CustomerSubscriptionResponseDto>>> GetByCustomer(Guid customerId)
    {
        var result = await _service.GetByCustomerIdAsync(customerId);
        return Ok(result);
    }

    /// <summary>Lista suscripciones sin cliente asignado. Filtrar por tipoServicioId para el selector de la factura.</summary>
    [HttpGet("unassigned")]
    public async Task<ActionResult<IEnumerable<CustomerSubscriptionResponseDto>>> GetUnassigned(
        [FromQuery] Guid? tipoServicioId = null)
    {
        var result = await _service.GetUnassignedAsync(tipoServicioId);
        return Ok(result);
    }

    /// <summary>Lista todas las suscripciones activas.</summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<CustomerSubscriptionResponseDto>>> GetActive()
    {
        var result = await _service.GetActiveAsync();
        return Ok(result);
    }

    /// <summary>Crea una nueva suscripción. Sin cliente → Status Unassigned.</summary>
    [HttpPost]
    public async Task<ActionResult<CustomerSubscriptionResponseDto>> Create([FromBody] CustomerSubscriptionCreateDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto, GetUserId());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Actualiza credenciales/estado de una suscripción.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerSubscriptionResponseDto>> Update(Guid id, [FromBody] CustomerSubscriptionUpdateDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Cancela una suscripción.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _service.CancelAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Asigna un cliente a una suscripción Unassigned → Active.
    /// Genera factura si se envía método de pago.
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<CustomerSubscriptionResponseDto>> Assign(Guid id, [FromBody] AssignCustomerDto dto)
    {
        try
        {
            var result = await _service.AssignCustomerAsync(id, dto, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Renueva una suscripción: marca la actual como Renewed y crea una nueva.
    /// Genera factura si se envía método de pago.
    /// </summary>
    [HttpPost("{id:guid}/renew")]
    public async Task<ActionResult<RenewalResponseDto>> Renew(Guid id, [FromBody] RenewalCreateDto dto)
    {
        try
        {
            var result = await _service.RenewAsync(id, dto, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
