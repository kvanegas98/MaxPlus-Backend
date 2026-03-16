using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LogsController : ControllerBase
{
    private readonly ISystemLogRepository _repository;

    public LogsController(ISystemLogRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene los logs del sistema. Filtra por nivel y/o fuente.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SystemLog>>> GetAll(
        [FromQuery] string? level  = null,
        [FromQuery] string? source = null,
        [FromQuery] int     top    = 200)
    {
        var logs = await _repository.GetAllAsync(level, source, top);
        return Ok(logs);
    }
}
