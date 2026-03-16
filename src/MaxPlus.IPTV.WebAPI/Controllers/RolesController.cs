using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

/// <summary>
/// Gestión de roles del sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Obtiene todos los roles activos del sistema.
    /// </summary>
    /// <remarks>Usado al crear o editar un usuario para seleccionar su rol.</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }
}
