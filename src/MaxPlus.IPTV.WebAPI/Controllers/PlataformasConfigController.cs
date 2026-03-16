using MaxPlus.IPTV.Application.DTOs.PlataformaConfig;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[Route("api/plataformas-config")]
[ApiController]
[Authorize(Roles = "Admin")]
public class PlataformasConfigController : ControllerBase
{
    private readonly IPlataformaConfigService _service;

    public PlataformasConfigController(IPlataformaConfigService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas las plataformas activas con su configuración de campos.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PlataformaConfigResponseDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>
    /// Obtiene una plataforma por su Id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlataformaConfigResponseDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null) return NotFound("Plataforma no encontrada.");
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva plataforma.
    /// Body: { plataforma, nombreAmigable, labelUsuario, tieneUrl, tienePin }
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] PlataformaConfigUpsertDto dto)
    {
        var newId = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId });
    }

    /// <summary>
    /// Actualiza la configuración de una plataforma existente.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PlataformaConfigUpsertDto dto)
    {
        try
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Desactiva una plataforma (no se elimina, queda inactiva).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _service.DeactivateAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
