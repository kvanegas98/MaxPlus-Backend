using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly IDemoService _demoService;

    public DemoController(IDemoService demoService)
    {
        _demoService = demoService;
    }

    /// <summary>
    /// Solicita una demo IPTV gratuita (accesible desde el menú digital).
    /// </summary>
    [HttpPost("request")]
    [AllowAnonymous]
    public async Task<ActionResult<DemoRequestResponseDto>> RequestDemo([FromBody] DemoRequestCreateDto dto)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var country = ""; // Futuro: resolver con un GeoIP Lite DB o similar API externa.

            var result = await _demoService.RequestDemoAsync(dto, ipAddress, country);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verifica el código OTP enviado al cliente.
    /// </summary>
    [HttpPost("{id:guid}/verify")]
    [AllowAnonymous]
    public async Task<ActionResult<DemoRequestResponseDto>> VerifyPhone(Guid id, [FromBody] DemoVerifyDto dto)
    {
        try
        {
            var result = await _demoService.VerifyPhoneAsync(id, dto.Code);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lista todas las solicitudes de demo.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DemoRequestResponseDto>>> GetAll([FromQuery] string? status = null)
    {
        var result = await _demoService.GetAllAsync(status);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una solicitud de demo por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<DemoRequestResponseDto>> GetById(Guid id)
    {
        var result = await _demoService.GetByIdAsync(id);
        if (result is null) return NotFound(new { message = $"Demo request con ID {id} no encontrada." });
        return Ok(result);
    }

    /// <summary>
    /// Aprueba una solicitud de demo.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DemoRequestResponseDto>> Approve(Guid id, [FromBody] DemoApproveDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized(new { message = "No se pudo identificar al usuario." });

        try
        {
            var result = await _demoService.ApproveAsync(id, userId, dto);
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
    /// Rechaza una solicitud de demo.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] DemoRejectDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized(new { message = "No se pudo identificar al usuario." });

        try
        {
            await _demoService.RejectAsync(id, userId, dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
