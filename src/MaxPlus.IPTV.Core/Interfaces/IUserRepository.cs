using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<Guid> CreateAsync(User user);
    Task       UpdateAsync(User user);
    Task       ChangePasswordAsync(Guid id, string newHash);
    Task       UpdateLastLoginAsync(Guid id);
    Task       DeactivateAsync(Guid id);
}
