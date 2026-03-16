using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaxPlus.IPTV.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService    _reportService;
    private readonly IDailyReportJob   _dailyReportJob;
    private readonly IPdfTicketService _pdfTicketService;

    public ReportsController(IReportService reportService, IDailyReportJob dailyReportJob, IPdfTicketService pdfTicketService)
    {
        _reportService    = reportService;
        _dailyReportJob   = dailyReportJob;
        _pdfTicketService = pdfTicketService;
    }

    /// <summary>
    /// KPIs y ventas por tipo de orden.
    /// </summary>
    /// <param name="filter">Hoy | Semana</param>
    [HttpGet("summary")]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary([FromQuery] string filter = "Hoy")
    {
        var result = await _reportService.GetSummaryAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Ventas agrupadas por hora (solo hoy).
    /// </summary>
    [HttpGet("sales-by-hour")]
    public async Task<ActionResult<IEnumerable<HourlySaleDto>>> GetSalesByHour()
    {
        var result = await _reportService.GetSalesByHourAsync();
        return Ok(result);
    }

    /// <summary>
    /// Servicios más vendidos.
    /// </summary>
    /// <param name="filter">Hoy | Semana</param>
    /// <param name="top">Cantidad de servicios (default 5)</param>
    [HttpGet("top-products")]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts(
        [FromQuery] string filter = "Hoy",
        [FromQuery] int top = 5)
    {
        var result = await _reportService.GetTopProductsAsync(filter, top);
        return Ok(result);
    }

    [HttpGet("order-history")]
    public async Task<ActionResult<IEnumerable<OrderHistoryDto>>> GetOrderHistory(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? customerName = null,
        [FromQuery] string? orderNumber = null)
    {
        var result = await _reportService.GetOrderHistoryAsync(startDate, endDate, pageNumber, pageSize, customerName, orderNumber);
        return Ok(result);
    }

    /// <summary>
    /// Dispara el envío del reporte diario de ventas inmediatamente (uso para pruebas).
    /// </summary>
    [HttpPost("enviar-reporte-diario")]
    public async Task<IActionResult> SendDailyReport(CancellationToken ct)
    {
        await _dailyReportJob.SendDailyReportAsync(ct);
        return Ok(new { message = "Reporte enviado exitosamente." });
    }

    /// <summary>
    /// Detalle completo de una factura por ID.
    /// </summary>
    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var invoice = await _reportService.GetInvoiceByIdAsync(id);
        if (invoice is null)
            return NotFound(new { message = "Factura no encontrada." });
        return Ok(invoice);
    }

    /// <summary>
    /// Descarga la factura como PDF.
    /// </summary>
    [HttpGet("invoices/{id:guid}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(Guid id)
    {
        var ticket = await _pdfTicketService.GenerateTicketAsync(id);
        return File(ticket.Content, "application/pdf", $"{ticket.OrderNumber}.pdf");
    }

    /// <summary>
    /// Descarga la factura como imagen PNG.
    /// </summary>
    [HttpGet("invoices/{id:guid}/png")]
    public async Task<IActionResult> DownloadInvoicePng(Guid id)
    {
        var ticket = await _pdfTicketService.GenerateTicketImageAsync(id);
        return File(ticket.Content, "image/png", $"{ticket.OrderNumber}.png");
    }

    /// <summary>
    /// Anula una factura.
    /// </summary>
    [HttpDelete("order-history/{id:guid}")]
    public async Task<IActionResult> VoidInvoice(Guid id, [FromBody] VoidRequestDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "No se pudo identificar al usuario de la sesión." });
        }

        try
        {
            await _reportService.VoidInvoiceAsync(id, userId, dto.Reason);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
