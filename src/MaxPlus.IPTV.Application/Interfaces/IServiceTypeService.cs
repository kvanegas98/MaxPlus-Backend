using MaxPlus.IPTV.Application.DTOs.ServiceType;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IServiceTypeService
{
    Task<IEnumerable<ServiceTypeDto>> GetAllAsync();
    Task<ServiceTypeDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(ServiceTypeCreateDto dto);
    Task UpdateAsync(Guid id, ServiceTypeUpdateDto dto);
    Task DeactivateAsync(Guid id);
    Task<IEnumerable<PlataformaConfigDto>> GetPlataformasConfigAsync();
}
