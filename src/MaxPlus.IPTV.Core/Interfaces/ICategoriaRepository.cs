using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> GetAllAsync();
    Task<IEnumerable<Categoria>> GetActivasAsync();
    Task<Categoria?>             GetByIdAsync(Guid id);
    Task<Guid>                   CreateAsync(Categoria categoria);
    Task                         UpdateAsync(Categoria categoria);
    Task                         DeleteAsync(Guid id);
}
