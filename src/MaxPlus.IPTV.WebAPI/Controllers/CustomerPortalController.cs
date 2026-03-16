using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MaxPlus.IPTV.WebAPI.Controllers;

/// <summary>
/// Portal del cliente: consulta suscripciones, demos y facturas por número de teléfono.
/// No requiere autenticación — el teléfono actúa como identificador.
/// </summary>
[ApiController]
[Route("api/portal")]
[AllowAnonymous]
[EnableRateLimiting("fixed-general")]
public class CustomerPortalController : ControllerBase
{
    private readonly ICustomerPortalService _service;

    public CustomerPortalController(ICustomerPortalService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna suscripciones, demos y facturas del cliente identificado por su ID.
    /// </summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup([FromQuery] Guid customerId)
    {
        if (customerId == Guid.Empty)
            return BadRequest(new { message = "El ID de cliente es requerido." });

        var result = await _service.GetByCustomerIdAsync(customerId);

        if (result is null)
            return NotFound(new { message = "No encontramos una cuenta con ese ID." });

        return Ok(result);
    }
}
