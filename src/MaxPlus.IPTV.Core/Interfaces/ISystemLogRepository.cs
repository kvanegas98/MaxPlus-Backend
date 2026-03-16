using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface ISystemLogRepository
{
    Task LogAsync(string level, string source, string message, string? details = null);
    Task<IEnumerable<SystemLog>> GetAllAsync(string? level = null, string? source = null, int top = 200);
}
