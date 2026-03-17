using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repository;

    public CategoriaService(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoriaResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<CategoriaResponseDto>> GetActivasAsync()
    {
        var items = await _repository.GetActivasAsync();
        return items.Select(MapToDto);
    }

    public async Task<CategoriaResponseDto?> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : MapToDto(item);
    }

    public async Task<CategoriaResponseDto> CreateAsync(CategoriaCreateDto dto)
    {
        var categoria = new Categoria
        {
            Nombre      = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Color       = dto.Color,
            Orden       = dto.Orden,
            IsActive    = true
        };

        categoria.Id = await _repository.CreateAsync(categoria);
        return MapToDto(categoria);
    }

    public async Task<CategoriaResponseDto> UpdateAsync(Guid id, CategoriaUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Categoría con ID {id} no encontrada.");

        existing.Nombre      = dto.Nombre.Trim();
        existing.Descripcion = dto.Descripcion?.Trim();
        existing.Color       = dto.Color;
        existing.Orden       = dto.Orden;
        existing.IsActive    = dto.IsActive;

        await _repository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Categoría con ID {id} no encontrada.");

        await _repository.DeleteAsync(existing.Id);
    }

    private static CategoriaResponseDto MapToDto(Categoria c) => new()
    {
        Id          = c.Id,
        Nombre      = c.Nombre,
        Descripcion = c.Descripcion,
        Color       = c.Color,
        Orden       = c.Orden,
        IsActive    = c.IsActive,
        CreatedAt   = c.CreatedAt
    };
}
