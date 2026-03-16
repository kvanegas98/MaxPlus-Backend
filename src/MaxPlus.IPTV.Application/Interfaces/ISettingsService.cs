using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface ISettingsService
{
    Task<SettingsResponseDto> GetAsync();
    Task<PublicSettingsDto> GetPublicAsync();
    Task<SettingsResponseDto> UpdateAsync(SettingsUpdateDto dto);
}
