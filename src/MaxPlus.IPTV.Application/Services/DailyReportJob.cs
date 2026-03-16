using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MaxPlus.IPTV.Application.Services;

public class DailyReportJob : IDailyReportJob
{
    private readonly IReportService  _reportService;
    private readonly IEmailService   _emailService;
    private readonly EmailSettings   _emailSettings;
    private readonly ILogger<DailyReportJob> _logger;

    public DailyReportJob(
        IReportService reportService,
        IEmailService emailService,
        IOptions<EmailSettings> emailOptions,
        ILogger<DailyReportJob> logger)
    {
        _reportService = reportService;
        _emailService  = emailService;
        _emailSettings = emailOptions.Value;
        _logger        = logger;
    }

    public async Task SendDailyReportAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Generando reporte diario de ventas IPTV...");

        var summary = await _reportService.GetSummaryAsync("Hoy");

        if (summary.TotalOrders == 0 && summary.TotalDemosToday == 0)
        {
            _logger.LogInformation("Sin ventas ni demos registradas hoy. No se enviará el reporte.");
            return;
        }

        var topProducts = await _reportService.GetTopProductsAsync("Hoy", 5);
        var byHour      = await _reportService.GetSalesByHourAsync();

        var peakHour = byHour
            .OrderByDescending(h => h.TotalSales)
            .FirstOrDefault();

        var html    = BuildHtml(summary, topProducts.ToList(), peakHour);
        var today   = DateTime.Now.ToString("dd 'de' MMMM yyyy", new System.Globalization.CultureInfo("es-NI"));
        var subject = $"Reporte de Ventas — MaxPlus IPTV — {today}";

        var recipients = _emailSettings.ReportRecipients.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
        if (recipients.Count == 0)
            recipients.Add(_emailSettings.FromEmail);

        await _emailService.SendAsync(recipients, subject, html);

        _logger.LogInformation("Reporte diario enviado a: {Recipients}.", string.Join(", ", recipients));
    }

    // ── HTML Builder ────────────────────────────────────────────────────────────
    private static string BuildHtml(
        ReportSummaryDto summary,
        List<TopProductDto> topProducts,
        HourlySaleDto? peakHour)
    {
        var today       = DateTime.Now.ToString("dddd, dd 'de' MMMM yyyy", new System.Globalization.CultureInfo("es-NI"));
        var generatedAt = DateTime.Now.ToString("hh:mm tt");

        // Top products rows
        var productRows = topProducts.Any()
            ? string.Join("", topProducts.Select((p, i) =>
                $"""
                <tr style="background:{(i % 2 == 0 ? "#ffffff" : "#f9fafb")};">
                  <td style="padding:10px 14px;color:#374151;">{i + 1}</td>
                  <td style="padding:10px 14px;color:#111827;font-weight:500;">{System.Net.WebUtility.HtmlEncode(p.Name)}</td>
                  <td style="padding:10px 14px;text-align:center;color:#374151;">{p.UnitsSold}</td>
                  <td style="padding:10px 14px;text-align:right;color:#059669;font-weight:600;">C$ {p.TotalRevenue:N2}</td>
                </tr>
                """))
            : """<tr><td colspan="4" style="padding:14px;text-align:center;color:#9ca3af;">Sin ventas registradas</td></tr>""";

        // Order type rows
        var orderTypeRows = summary.ByOrderType.Any()
            ? string.Join("", summary.ByOrderType.Select(t =>
                $"""
                <tr>
                  <td style="padding:9px 14px;color:#374151;">{System.Net.WebUtility.HtmlEncode(t.OrderType)}</td>
                  <td style="padding:9px 14px;text-align:right;font-weight:600;color:#1e40af;">{t.TotalOrders}</td>
                </tr>
                """))
            : """<tr><td colspan="2" style="padding:14px;text-align:center;color:#9ca3af;">Sin datos</td></tr>""";

        var peakText = peakHour is not null
            ? $"{peakHour.Hour:D2}:00 – {(peakHour.Hour + 1):D2}:00 (C$ {peakHour.TotalSales:N2})"
            : "—";

        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8"/>
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <title>Reporte de Ventas IPTV</title>
        </head>
        <body style="margin:0;padding:0;background:#f3f4f6;font-family:'Segoe UI',Arial,sans-serif;">

          <!-- WRAPPER -->
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f3f4f6;padding:32px 0;">
            <tr><td align="center">
              <table width="620" cellpadding="0" cellspacing="0" style="max-width:620px;width:100%;">

                <!-- HEADER -->
                <tr>
                  <td style="background:linear-gradient(135deg,#7c3aed,#4f46e5);border-radius:12px 12px 0 0;padding:36px 40px;text-align:center;">
                    <div style="font-size:28px;font-weight:800;color:#ffffff;letter-spacing:1px;">📺 MaxPlus IPTV</div>
                    <div style="font-size:13px;color:#c4b5fd;margin-top:6px;text-transform:uppercase;letter-spacing:2px;">Reporte Diario de Ventas</div>
                    <div style="font-size:14px;color:#ddd6fe;margin-top:8px;">{{today}}</div>
                  </td>
                </tr>

                <!-- BODY -->
                <tr>
                  <td style="background:#ffffff;padding:36px 40px;">

                    <!-- KPI CARDS -->
                    <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:32px;">
                      <tr>
                        <!-- Ventas Totales -->
                        <td width="48%" style="background:#f0fdf4;border:1px solid #bbf7d0;border-radius:10px;padding:18px 16px;text-align:center;vertical-align:top;">
                          <div style="font-size:11px;color:#6b7280;text-transform:uppercase;letter-spacing:1px;margin-bottom:6px;">Ventas Totales</div>
                          <div style="font-size:26px;font-weight:800;color:#15803d;">C$ {{summary.TotalSales:N2}}</div>
                        </td>
                        <td width="4%"></td>
                        <!-- Total Órdenes -->
                        <td width="48%" style="background:#eff6ff;border:1px solid #bfdbfe;border-radius:10px;padding:18px 16px;text-align:center;vertical-align:top;">
                          <div style="font-size:11px;color:#6b7280;text-transform:uppercase;letter-spacing:1px;margin-bottom:6px;">Total Órdenes</div>
                          <div style="font-size:26px;font-weight:800;color:#1d4ed8;">{{summary.TotalOrders}}</div>
                        </td>
                      </tr>
                      <tr><td colspan="3" style="height:12px;"></td></tr>
                      <tr>
                        <!-- Demos Generadas -->
                        <td width="48%" style="background:#faf5ff;border:1px solid #e9d5ff;border-radius:10px;padding:18px 16px;text-align:center;vertical-align:top;">
                          <div style="font-size:11px;color:#6b7280;text-transform:uppercase;letter-spacing:1px;margin-bottom:6px;">Demos Hoy</div>
                          <div style="font-size:26px;font-weight:800;color:#7c3aed;">{{summary.TotalDemosToday}}</div>
                        </td>
                        <td width="4%"></td>
                        <!-- Suscripciones Activas -->
                        <td width="48%" style="background:#fefce8;border:1px solid #fde68a;border-radius:10px;padding:18px 16px;text-align:center;vertical-align:top;">
                          <div style="font-size:11px;color:#6b7280;text-transform:uppercase;letter-spacing:1px;margin-bottom:6px;">Suscripciones Activas</div>
                          <div style="font-size:26px;font-weight:800;color:#b45309;">{{summary.ActiveSubscriptions}}</div>
                        </td>
                      </tr>
                      <tr><td colspan="3" style="height:12px;"></td></tr>
                      <tr>
                        <!-- Ticket Promedio -->
                        <td width="48%" style="background:#f0f9ff;border:1px solid #bae6fd;border-radius:10px;padding:18px 16px;text-align:center;vertical-align:top;">
                          <div style="font-size:11px;color:#6b7280;text-transform:uppercase;letter-spacing:1px;margin-bottom:6px;">Ticket Promedio</div>
                          <div style="font-size:26px;font-weight:800;color:#0369a1;">C$ {{summary.AverageTicket:N2}}</div>
                        </td>
                        <td width="4%"></td>
                        <!-- Hora Pico -->
                        <td width="48%" style="background:#fdf2f8;border:1px solid #f9a8d4;border-radius:10px;padding:18px 16px;text-align:center;vertical-align:top;">
                          <div style="font-size:11px;color:#6b7280;text-transform:uppercase;letter-spacing:1px;margin-bottom:6px;">Hora Pico</div>
                          <div style="font-size:15px;font-weight:700;color:#9d174d;margin-top:4px;">{{peakText}}</div>
                        </td>
                      </tr>
                    </table>

                    <!-- PRODUCTO ESTRELLA -->
                    {{(string.IsNullOrWhiteSpace(summary.TopSellingProduct) ? "" : $"""
                    <div style="background:#faf5ff;border-left:4px solid #7c3aed;border-radius:0 8px 8px 0;padding:14px 18px;margin-bottom:28px;">
                      <span style="font-size:12px;color:#6b21a8;text-transform:uppercase;letter-spacing:1px;">⭐ Servicio Estrella</span>
                      <div style="font-size:18px;font-weight:700;color:#581c87;margin-top:4px;">{System.Net.WebUtility.HtmlEncode(summary.TopSellingProduct)}</div>
                    </div>
                    """)}}

                    <!-- TOP SERVICIOS -->
                    <div style="font-size:14px;font-weight:700;color:#111827;margin-bottom:12px;border-bottom:2px solid #e5e7eb;padding-bottom:8px;">
                      Top 5 Servicios del Día
                    </div>
                    <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;margin-bottom:28px;border-radius:8px;overflow:hidden;border:1px solid #e5e7eb;">
                      <thead>
                        <tr style="background:#f9fafb;">
                          <th style="padding:10px 14px;text-align:left;font-size:12px;color:#6b7280;font-weight:600;">#</th>
                          <th style="padding:10px 14px;text-align:left;font-size:12px;color:#6b7280;font-weight:600;">Servicio</th>
                          <th style="padding:10px 14px;text-align:center;font-size:12px;color:#6b7280;font-weight:600;">Unidades</th>
                          <th style="padding:10px 14px;text-align:right;font-size:12px;color:#6b7280;font-weight:600;">Total</th>
                        </tr>
                      </thead>
                      <tbody>
                        {{productRows}}
                      </tbody>
                    </table>

                    <!-- ÓRDENES POR TIPO -->
                    <div style="font-size:14px;font-weight:700;color:#111827;margin-bottom:12px;border-bottom:2px solid #e5e7eb;padding-bottom:8px;">
                      Órdenes por Tipo
                    </div>
                    <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;border:1px solid #e5e7eb;border-radius:8px;overflow:hidden;">
                      <thead>
                        <tr style="background:#f9fafb;">
                          <th style="padding:10px 14px;text-align:left;font-size:12px;color:#6b7280;font-weight:600;">Tipo de Orden</th>
                          <th style="padding:10px 14px;text-align:right;font-size:12px;color:#6b7280;font-weight:600;">Órdenes</th>
                        </tr>
                      </thead>
                      <tbody>
                        {{orderTypeRows}}
                      </tbody>
                    </table>

                  </td>
                </tr>

                <!-- FOOTER -->
                <tr>
                  <td style="background:#f9fafb;border-top:1px solid #e5e7eb;border-radius:0 0 12px 12px;padding:20px 40px;text-align:center;">
                    <p style="margin:0;font-size:12px;color:#9ca3af;">Generado automáticamente a las {{generatedAt}} (hora Nicaragua)</p>
                    <p style="margin:6px 0 0;font-size:12px;color:#d1d5db;">MaxPlus IPTV &mdash; Sistema de Gestión</p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>

        </body>
        </html>
        """;
    }
}
