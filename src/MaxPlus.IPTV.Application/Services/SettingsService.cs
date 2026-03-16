using MaxPlus.IPTV.Application.Helpers;
using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;

    public SettingsService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<SettingsResponseDto> GetAsync()
    {
        var settings = await _settingsRepository.GetAsync()
            ?? throw new Exception("No se encontró la configuración del sistema.");

        return MapToDto(settings);
    }

    public async Task<PublicSettingsDto> GetPublicAsync()
    {
        var settings = await _settingsRepository.GetAsync()
            ?? throw new Exception("No se encontró la configuración del sistema.");

        return MapToPublicDto(settings);
    }

    public async Task<SettingsResponseDto> UpdateAsync(SettingsUpdateDto dto)
    {
        var settings = new Settings
        {
            BusinessName          = dto.BusinessName.Trim(),
            Phone                 = dto.Phone?.Trim(),
            Description           = dto.Description?.Trim(),
            Address               = dto.Address?.Trim(),
            LogoUrl               = dto.LogoUrl?.Trim(),
            ExchangeRateUSD       = dto.ExchangeRateUSD,
            DemoPhpBaseUrl        = dto.DemoPhpBaseUrl?.Trim(),
            PublicMenuEnabled     = dto.PublicMenuEnabled,
            DemoAutoApprove       = dto.DemoAutoApprove
        };

        await _settingsRepository.UpdateAsync(settings);

        settings.UpdatedAt = DateTimeHelper.GetNicaraguaTime();
        return MapToDto(settings);
    }

    private static SettingsResponseDto MapToDto(Settings s) => new()
    {
        BusinessName          = s.BusinessName,
        Phone                 = s.Phone,
        Description           = s.Description,
        Address               = s.Address,
        LogoUrl               = s.LogoUrl,
        ExchangeRateUSD       = s.ExchangeRateUSD,
        DemoPhpBaseUrl        = s.DemoPhpBaseUrl,
        PublicMenuEnabled     = s.PublicMenuEnabled,
        DemoAutoApprove       = s.DemoAutoApprove,
        UpdatedAt             = s.UpdatedAt
    };

    private static PublicSettingsDto MapToPublicDto(Settings s) => new()
    {
        BusinessName      = s.BusinessName,
        Phone             = s.Phone,
        Description       = s.Description,
        Address           = s.Address,
        LogoUrl           = s.LogoUrl,
        ExchangeRateUSD   = s.ExchangeRateUSD,
        PublicMenuEnabled = s.PublicMenuEnabled,
        DemoAutoApprove   = s.DemoAutoApprove
    };
}
