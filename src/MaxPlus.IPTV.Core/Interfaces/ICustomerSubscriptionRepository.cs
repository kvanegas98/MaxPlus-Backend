using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface ICustomerSubscriptionRepository
{
    Task<IEnumerable<CustomerSubscription>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<CustomerSubscription>> GetActiveAsync();
    Task<IEnumerable<CustomerSubscription>> GetUnassignedAsync(Guid? tipoServicioId = null);
    Task<CustomerSubscription?>             GetByIdAsync(Guid id);
    Task<IEnumerable<CustomerSubscription>> GetExpiringAsync(int daysAhead);
    Task<Guid>                              AddAsync(CustomerSubscription subscription);
    Task                                    AssignCustomerAsync(Guid id, Guid customerId);
    Task                                    UpdateAsync(CustomerSubscription subscription);
    Task                                    CancelAsync(Guid id);
    Task                                    MarkNotifiedAsync(Guid id, int daysAhead);
    Task<int>                               ExpireOldAsync();
    Task<Guid>                              RenewAsync(Guid subscriptionId, DateTime newExpiration);
}
