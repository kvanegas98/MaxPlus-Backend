namespace MaxPlus.IPTV.Application.DTOs;

public class PdfTicketResponseDto
{
    public byte[] Content     { get; set; } = Array.Empty<byte>();
    public string OrderNumber { get; set; } = string.Empty;
}
