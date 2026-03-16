namespace MaxPlus.IPTV.Core.Entities;

public class SystemLog
{
    public Guid     Id        { get; set; }
    public string   Level     { get; set; } = "Info";  // Info | Warning | Error
    public string   Source    { get; set; } = string.Empty;
    public string   Message   { get; set; } = string.Empty;
    public string?  Details   { get; set; }
    public DateTime CreatedAt { get; set; }
}
