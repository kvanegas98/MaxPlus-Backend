using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IPdfTicketService
{
    Task<PdfTicketResponseDto> GenerateTicketAsync(Guid invoiceId);

    /// <summary>Genera el ticket de factura como imagen PNG (primera página del PDF).</summary>
    Task<PdfTicketResponseDto> GenerateTicketImageAsync(Guid invoiceId);
}
