using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IPlataformaConfigRepository
{
    Task<IEnumerable<PlataformaConfig>> GetAllAsync();
    Task<PlataformaConfig?> GetByIdAsync(int id);
    Task<int> CreateAsync(PlataformaConfig config);
    Task UpdateAsync(PlataformaConfig config);
    Task DeactivateAsync(int id);
}
