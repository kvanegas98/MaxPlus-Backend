namespace MaxPlus.IPTV.Application.Interfaces;

public interface IDailyReportJob
{
    Task SendDailyReportAsync(CancellationToken ct = default);
}
