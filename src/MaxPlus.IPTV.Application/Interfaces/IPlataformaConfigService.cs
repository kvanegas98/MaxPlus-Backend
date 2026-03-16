using MaxPlus.IPTV.Application.DTOs.PlataformaConfig;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IPlataformaConfigService
{
    Task<IEnumerable<PlataformaConfigResponseDto>> GetAllAsync();
    Task<PlataformaConfigResponseDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(PlataformaConfigUpsertDto dto);
    Task UpdateAsync(int id, PlataformaConfigUpsertDto dto);
    Task DeactivateAsync(int id);
}
