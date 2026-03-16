using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IOrderService
{
    Task<ServiceOrderResponseDto> CreateAsync(ServiceOrderCreateDto dto, string? ipAddress);
    Task<ServiceOrderResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceOrderResponseDto>> GetAllAsync(string? status = null);
    Task<ServiceOrderResponseDto> ApproveAsync(Guid id, Guid approvedBy, ServiceOrderApproveDto dto);
    Task RejectAsync(Guid id, Guid approvedBy, ServiceOrderRejectDto dto);
}
