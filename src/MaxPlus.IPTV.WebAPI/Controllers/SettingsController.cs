using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Gerente")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<ActionResult<SettingsResponseDto>> Get()
    {
        var settings = await _settingsService.GetAsync();
        return Ok(settings);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicSettingsDto>> GetPublic()
    {
        var settings = await _settingsService.GetPublicAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<ActionResult<SettingsResponseDto>> Update([FromBody] SettingsUpdateDto dto)
    {
        var result = await _settingsService.UpdateAsync(dto);
        return Ok(result);
    }
}
