using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    /// <summary>
    /// Resumen financiero y operativo. Parámetros opcionales de fecha (default: mes actual).
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        var result = await _service.GetSummaryAsync(fechaDesde, fechaHasta);
        return Ok(result);
    }

    /// <summary>
    /// Lista de suscripciones que vencen en los próximos N días (default: 30).
    /// </summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring([FromQuery] int days = 30)
    {
        if (days < 1 || days > 365)
            return BadRequest(new { message = "El parámetro 'days' debe estar entre 1 y 365." });

        var result = await _service.GetExpiringAsync(days);
        return Ok(result);
    }
}
