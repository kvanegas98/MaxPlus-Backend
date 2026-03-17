using MaxPlus.IPTV.Application.DTOs.ServiceType;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ServiceTypesController : ControllerBase
{
    private readonly IServiceTypeService _service;
    private readonly IStorageService _storage;

    public ServiceTypesController(IServiceTypeService service, IStorageService storage)
    {
        _service = service;
        _storage = storage;
    }

    /// <summary>
    /// Devuelve la configuración de campos por plataforma (IPTV, FlujoTV, Netflix, Streaming).
    /// El frontend usa esto para mostrar dinámicamente los campos correctos al crear/editar credenciales.
    /// </summary>
    [HttpGet("plataformas-config")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PlataformaConfigDto>>> GetPlataformasConfig()
    {
        var result = await _service.GetPlataformasConfigAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los tipos de servicio (admin). Requiere token.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ServiceTypeDto>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Catálogo público — solo servicios activos, sin token.
    /// </summary>
    [HttpGet("catalogo")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ServiceTypeDto>>> GetCatalogo()
    {
        var result = await _service.GetCatalogoAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un tipo de servicio por su Id único.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceTypeDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound("Tipo de servicio no encontrado.");
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo tipo de servicio (Plan).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Solo administradores pueden crear
    public async Task<ActionResult<Guid>> Create([FromBody] ServiceTypeCreateDto dto)
    {
        var newId = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId });
    }

    /// <summary>
    /// Actualiza la información y precios de un servicio existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ServiceTypeUpdateDto dto)
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
    /// Sube la imagen de un servicio a Cloudinary (carpeta Maxplus/Servicios) y devuelve la URL.
    /// </summary>
    [HttpPost("upload-image")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No se proporcionó ninguna imagen." });

        using var stream = file.OpenReadStream();
        var url = await _storage.UploadFileAsync(stream, file.FileName, "Maxplus/Servicios");
        return Ok(new { imageUrl = url });
    }

    /// <summary>
    /// Oculta o desactiva (soft-delete) un servicio del catálogo.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deletedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            await _service.DeleteAsync(id, deletedBy);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
