using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface ICustomerSubscriptionService
{
    Task<IEnumerable<CustomerSubscriptionResponseDto>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<CustomerSubscriptionResponseDto>> GetActiveAsync();
    Task<IEnumerable<CustomerSubscriptionResponseDto>> GetUnassignedAsync();
    Task<CustomerSubscriptionResponseDto>              CreateAsync(CustomerSubscriptionCreateDto dto, Guid userId);
    Task<CustomerSubscriptionResponseDto>              AssignCustomerAsync(Guid subscriptionId, AssignCustomerDto dto, Guid userId);
    Task<CustomerSubscriptionResponseDto>              UpdateAsync(Guid id, CustomerSubscriptionUpdateDto dto);
    Task                                               CancelAsync(Guid id);
    Task<RenewalResponseDto>                           RenewAsync(Guid subscriptionId, RenewalCreateDto dto, Guid userId);
}
