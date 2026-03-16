using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IIptvAccountRepository
{
    Task<IEnumerable<IptvAccount>>        GetAllAsync();
    Task<IptvAccount?>                    GetByIdAsync(Guid id);
    Task<IEnumerable<IptvAccountSlotRow>> GetWithClientsAsync();
    Task<IEnumerable<IptvAccount>>        GetByServiceTypeAsync(Guid tipoServicioId);
    Task<Guid>                            AddAsync(IptvAccount account);
    Task                                  UpdateAsync(IptvAccount account);
    Task                                  DeactivateAsync(Guid id);
    Task<Guid>                            AssignClientAsync(Guid accountId, Guid customerId, DateTime expirationDate, Guid? tipoServicioId, string? profileUser, string? profilePin);
    Task<(int totalAccounts, int totalSlots, int usedSlots)> GetStatsAsync();
}
