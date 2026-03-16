using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Crea una nueva orden de servicio desde el menú digital (público).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceOrderResponseDto>> Create([FromBody] ServiceOrderCreateDto dto)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _orderService.CreateAsync(dto, ip);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lista todas las órdenes, con filtro opcional por status: Pending | Approved | Rejected
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ServiceOrderResponseDto>>> GetAll([FromQuery] string? status = null)
    {
        var result = await _orderService.GetAllAsync(status);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una orden por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ServiceOrderResponseDto>> GetById(Guid id)
    {
        var result = await _orderService.GetByIdAsync(id);
        if (result is null) return NotFound(new { message = $"Orden con ID {id} no encontrada." });
        return Ok(result);
    }

    /// <summary>
    /// Aprueba la orden: crea el cliente (si no existe) y la suscripción, y envía las credenciales por email.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceOrderResponseDto>> Approve(Guid id, [FromBody] ServiceOrderApproveDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized(new { message = "No se pudo identificar al usuario." });

        try
        {
            var result = await _orderService.ApproveAsync(id, userId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Rechaza la orden y notifica al cliente por email.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ServiceOrderRejectDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized(new { message = "No se pudo identificar al usuario." });

        try
        {
            await _orderService.RejectAsync(id, userId, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}
