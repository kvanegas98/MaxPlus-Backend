using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?>             GetByIdAsync(Guid id);
    Task<IEnumerable<Customer>> SearchAsync(string term);
    Task<Customer?>             FindByContactAsync(string? email, string? phone);
    Task<Guid>                  AddAsync(Customer customer);
    Task                        UpdateAsync(Customer customer);
    Task                        DeleteAsync(Guid id);
}
