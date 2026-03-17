using MaxPlus.IPTV.Application.DTOs.ServiceType;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class ServiceTypeService : IServiceTypeService
{
    private readonly IServiceTypeRepository      _repository;
    private readonly IPlataformaConfigRepository _plataformaRepo;

    public ServiceTypeService(
        IServiceTypeRepository      repository,
        IPlataformaConfigRepository plataformaRepo)
    {
        _repository     = repository;
        _plataformaRepo = plataformaRepo;
    }

    public async Task<IEnumerable<ServiceTypeDto>> GetAllAsync()
    {
        var services = await _repository.GetAllAsync();
        return services.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceTypeDto>> GetCatalogoAsync()
    {
        var services = await _repository.GetCatalogoAsync();
        return services.Select(MapToDto);
    }

    public async Task<ServiceTypeDto?> GetByIdAsync(Guid id)
    {
        var serviceType = await _repository.GetByIdAsync(id);
        return serviceType == null ? null : MapToDto(serviceType);
    }

    public async Task<Guid> CreateAsync(ServiceTypeCreateDto dto)
    {
        var serviceType = new ServiceType
        {
            Name          = dto.Name,
            Description   = dto.Description,
            Price         = dto.Price,
            PurchasePrice = dto.PurchasePrice,
            DurationDays  = dto.DurationDays,
            Category      = dto.Category,
            Plataforma    = dto.Plataforma,
            ImageUrl      = dto.ImageUrl,
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow
        };

        return await _repository.CreateAsync(serviceType);
    }

    public async Task UpdateAsync(Guid id, ServiceTypeUpdateDto dto)
    {
        var existingService = await _repository.GetByIdAsync(id);
        if (existingService == null)
            throw new KeyNotFoundException("Tipo de servicio no encontrado.");

        existingService.Name          = dto.Name;
        existingService.Description   = dto.Description;
        existingService.Price         = dto.Price;
        existingService.PurchasePrice = dto.PurchasePrice;
        existingService.DurationDays  = dto.DurationDays;
        existingService.Category      = dto.Category;
        existingService.Plataforma    = dto.Plataforma;
        existingService.ImageUrl      = dto.ImageUrl;
        existingService.IsActive      = dto.IsActive;

        await _repository.UpdateAsync(existingService);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var existingService = await _repository.GetByIdAsync(id);
        if (existingService == null)
            throw new KeyNotFoundException("Tipo de servicio no encontrado.");

        await _repository.DeactivateAsync(id);
    }

    public async Task DeleteAsync(Guid id, string? deletedBy = null)
    {
        var existingService = await _repository.GetByIdAsync(id);
        if (existingService == null)
            throw new KeyNotFoundException("Tipo de servicio no encontrado.");

        await _repository.DeleteAsync(id, deletedBy);
    }

    public async Task<IEnumerable<PlataformaConfigDto>> GetPlataformasConfigAsync()
    {
        var configs = await _plataformaRepo.GetAllAsync();
        return configs.Select(MapPlataformaToDto);
    }

    private static PlataformaConfigDto MapPlataformaToDto(PlataformaConfig c)
    {
        // Campos se deriva de los flags — no hace falta almacenarlos en BD
        var campos = new List<string> { c.LabelUsuario == "Correo" ? "correo" : "usuario", "contraseña" };
        if (c.TienePin) campos.Add("pin");

        return new PlataformaConfigDto
        {
            Plataforma   = c.Plataforma,
            Campos       = [.. campos],
            LabelUsuario = c.LabelUsuario,
            TieneUrl     = c.TieneUrl,
            TienePin     = c.TienePin
        };
    }

    private static ServiceTypeDto MapToDto(ServiceType entity) => new()
    {
        Id            = entity.Id,
        Name          = entity.Name,
        Description   = entity.Description,
        Price         = entity.Price,
        PurchasePrice = entity.PurchasePrice,
        DurationDays  = entity.DurationDays,
        Category      = entity.Category,
        Plataforma    = entity.Plataforma,
        ImageUrl      = entity.ImageUrl,
        IsActive      = entity.IsActive
    };
}
