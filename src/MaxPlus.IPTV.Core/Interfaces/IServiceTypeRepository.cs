using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IServiceTypeRepository
{
    Task<IEnumerable<ServiceType>> GetAllAsync();
    Task<ServiceType?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(ServiceType serviceType);
    Task UpdateAsync(ServiceType serviceType);
    Task DeactivateAsync(Guid id);
}
