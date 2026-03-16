using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IDemoRequestRepository
{
    Task<Guid>                         AddAsync(DemoRequest request);
    Task<IEnumerable<DemoRequest>>     GetAllAsync(string? status = null);
    Task<DemoRequest?>                 GetByIdAsync(Guid id);
    Task                               ApproveAsync(Guid id, Guid? approvedBy, string? demoUrl, string? responseHtml);
    Task                               RejectAsync(Guid id, Guid approvedBy, string? rejectionReason);
    Task                               VerifyPhoneAsync(Guid id, string code);
    Task<IEnumerable<DemoRequest>>     GetByPhoneAsync(string phone);
}
