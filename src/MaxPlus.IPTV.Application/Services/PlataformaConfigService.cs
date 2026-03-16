using MaxPlus.IPTV.Application.DTOs.PlataformaConfig;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class PlataformaConfigService : IPlataformaConfigService
{
    private readonly IPlataformaConfigRepository _repository;

    public PlataformaConfigService(IPlataformaConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PlataformaConfigResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<PlataformaConfigResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : MapToDto(item);
    }

    public async Task<int> CreateAsync(PlataformaConfigUpsertDto dto)
    {
        var entity = new PlataformaConfig
        {
            Plataforma     = dto.Plataforma.Trim(),
            NombreAmigable = dto.NombreAmigable.Trim(),
            LabelUsuario   = dto.LabelUsuario.Trim(),
            TieneUrl       = dto.TieneUrl,
            TienePin       = dto.TienePin,
            TieneCorreo    = dto.TieneCorreo
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task UpdateAsync(int id, PlataformaConfigUpsertDto dto)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Plataforma con Id {id} no encontrada.");

        existing.Plataforma     = dto.Plataforma.Trim();
        existing.NombreAmigable = dto.NombreAmigable.Trim();
        existing.LabelUsuario   = dto.LabelUsuario.Trim();
        existing.TieneUrl       = dto.TieneUrl;
        existing.TienePin       = dto.TienePin;
        existing.TieneCorreo    = dto.TieneCorreo;

        await _repository.UpdateAsync(existing);
    }

    public async Task DeactivateAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Plataforma con Id {id} no encontrada.");

        await _repository.DeactivateAsync(existing.Id);
    }

    private static PlataformaConfigResponseDto MapToDto(PlataformaConfig c)
    {
        var campos = new List<string> { c.LabelUsuario == "Correo" ? "correo" : "usuario", "contraseña" };
        if (c.TieneCorreo) campos.Add("usuario");  // usuario interno del perfil (Netflix/Streaming)
        if (c.TienePin)    campos.Add("pin");
        if (c.TieneUrl)    campos.Add("url");

        return new PlataformaConfigResponseDto
        {
            Id             = c.Id,
            Plataforma     = c.Plataforma,
            NombreAmigable = c.NombreAmigable,
            LabelUsuario   = c.LabelUsuario,
            TieneUrl       = c.TieneUrl,
            TienePin       = c.TienePin,
            TieneCorreo    = c.TieneCorreo,
            Campos         = [.. campos]
        };
    }
}
