using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaResponseDto>> GetAllAsync();
    Task<IEnumerable<CategoriaResponseDto>> GetActivasAsync();
    Task<CategoriaResponseDto?>             GetByIdAsync(Guid id);
    Task<CategoriaResponseDto>              CreateAsync(CategoriaCreateDto dto);
    Task<CategoriaResponseDto>              UpdateAsync(Guid id, CategoriaUpdateDto dto);
    Task                                    DeleteAsync(Guid id);
}
