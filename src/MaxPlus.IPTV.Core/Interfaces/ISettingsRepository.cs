using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface ISettingsRepository
{
    Task<Settings?> GetAsync();
    Task            UpdateAsync(Settings settings);
}
