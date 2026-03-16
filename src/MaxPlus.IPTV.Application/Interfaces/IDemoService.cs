using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IDemoService
{
    Task<DemoRequestResponseDto>              RequestDemoAsync(DemoRequestCreateDto dto, string? ipAddress, string? country);
    Task<DemoRequestResponseDto?>             VerifyPhoneAsync(Guid id, string code);
    Task<IEnumerable<DemoRequestResponseDto>> GetAllAsync(string? status = null);
    Task<DemoRequestResponseDto?>             GetByIdAsync(Guid id);
    Task<DemoRequestResponseDto>              ApproveAsync(Guid id, Guid approvedBy, DemoApproveDto dto);
    Task                                      RejectAsync(Guid id, Guid approvedBy, DemoRejectDto dto);
}
