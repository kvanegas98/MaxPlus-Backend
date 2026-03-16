using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponseDto>> GetAllAsync();
    Task<CustomerResponseDto?>             GetByIdAsync(Guid id);
    Task<IEnumerable<CustomerResponseDto>> SearchAsync(string term);
    Task<CustomerResponseDto>              CreateAsync(CustomerCreateDto dto);
    Task<CustomerResponseDto>              UpdateAsync(Guid id, CustomerUpdateDto dto);
    Task                                   DeleteAsync(Guid id);
}
