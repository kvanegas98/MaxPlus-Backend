namespace MaxPlus.IPTV.Application.Interfaces;

public class PanelCreatedAccountDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}

public interface IIptvPanelClientService
{
    Task<PanelCreatedAccountDto> CreateTrialAccountAsync(string hostUrl, string username, string password, string customerName);
}
