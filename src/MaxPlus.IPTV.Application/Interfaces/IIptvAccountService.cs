using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IIptvAccountService
{
    Task<IEnumerable<IptvAccountResponseDto>>      GetAllAsync();
    Task<IptvAccountResponseDto>                   GetWithClientsAsync(Guid id);
    Task<PagedResult<IptvAccountResponseDto>>      GetAllWithClientsAsync(int page, int pageSize);
    Task<IEnumerable<IptvAccountAvailableDto>>     GetByServiceTypeAsync(Guid tipoServicioId);
    Task<IptvAccountResponseDto>                   CreateAsync(IptvAccountCreateDto dto);
    Task<IptvAccountResponseDto>                   UpdateAsync(Guid id, IptvAccountUpdateDto dto);
    Task                                           DeactivateAsync(Guid id);
    Task<IptvAccountAssignResponseDto>             AssignClientAsync(Guid accountId, IptvAccountAssignClientDto dto, Guid userId);
    Task<IptvAccountStatsDto>                      GetStatsAsync();
}
