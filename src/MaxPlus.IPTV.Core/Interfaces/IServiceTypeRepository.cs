using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IServiceTypeRepository
{
    Task<IEnumerable<ServiceType>> GetAllAsync();
    Task<IEnumerable<ServiceType>> GetCatalogoAsync();
    Task<ServiceType?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(ServiceType serviceType);
    Task UpdateAsync(ServiceType serviceType);
    Task DeactivateAsync(Guid id);
    Task DeleteAsync(Guid id, string? deletedBy = null);
}
