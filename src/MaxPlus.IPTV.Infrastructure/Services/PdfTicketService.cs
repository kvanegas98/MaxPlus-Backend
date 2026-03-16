using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Interfaces;
using PDFtoImage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace MaxPlus.IPTV.Infrastructure.Services;

public class PdfTicketService : IPdfTicketService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISettingsService   _settingsService;

    // ── Paleta — colores de la aplicación ──────────────────────────────────
    private const string DeepBg    = "#0D0B1E";   // fondo principal
    private const string CardBg    = "#1A1535";   // tarjetas
    private const string CardLine  = "#2D2A4A";   // separadores en tarjeta
    private const string Purple    = "#8B5CF6";   // color marca
    private const string Green     = "#10B981";   // acento verde (botones app)
    private const string Orange    = "#FF6B00";   // precio / destacado
    private const string LightBg   = "#F3F0FF";   // fondo alterno tabla
    private const string LineColor = "#DDD6FE";   // bordes tabla
    private const string TextSub   = "#4B5563";   // texto secundario tabla
    private const string TextMuted = "#9CA3AF";   // texto apagado
    private const string White     = "#FFFFFF";

    public PdfTicketService(IInvoiceRepository invoiceRepository, ISettingsService settingsService)
    {
        _invoiceRepository = invoiceRepository;
        _settingsService   = settingsService;
    }

    // ── PDF ─────────────────────────────────────────────────────────────────
    public async Task<PdfTicketResponseDto> GenerateTicketAsync(Guid invoiceId)
    {
        var invoice  = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId)
            ?? throw new KeyNotFoundException($"No se encontró la factura con ID {invoiceId}");

        var settings = await _settingsService.GetAsync();

        string? logoPath = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "Logo.png"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "assets", "Logo.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "Logo.png"),
        }.FirstOrDefault(File.Exists);

        decimal subtotal = invoice.Details.Sum(d => d.SubTotal) + invoice.DiscountAmount;

        bool tieneDatosBancarios = !string.IsNullOrWhiteSpace(invoice.MetodoPagoNumeroCuenta)
                                || !string.IsNullOrWhiteSpace(invoice.MetodoPagoBanco);

        var pdfData = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize(300, PageSizes.A4.Height));
                page.Margin(0);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial).FontColor(DeepBg));

                // ── ENCABEZADO ───────────────────────────────────────────────
                page.Header().Column(hdr =>
                {
                    hdr.Item().Height(5).Background(Purple);

                    hdr.Item().Background(DeepBg).PaddingHorizontal(16).PaddingVertical(14).Column(col =>
                    {
                        if (logoPath != null)
                            col.Item().AlignCenter().Height(56).Image(logoPath, ImageScaling.FitHeight);

                        col.Item().PaddingTop(logoPath != null ? 6 : 0)
                            .AlignCenter().Text(settings.BusinessName.ToUpper())
                            .FontSize(12).ExtraBold().FontColor(White);

                        if (!string.IsNullOrWhiteSpace(settings.Description))
                            col.Item().PaddingTop(2).AlignCenter().Text(settings.Description)
                                .FontSize(7).Italic().FontColor(Purple);

                        col.Item().PaddingTop(6).AlignCenter().Row(r =>
                        {
                            if (!string.IsNullOrWhiteSpace(settings.Phone))
                            {
                                r.AutoItem().Text($"Tel. {settings.Phone}").FontSize(7).FontColor(TextMuted);
                                if (!string.IsNullOrWhiteSpace(settings.Address))
                                    r.AutoItem().Text("   ·   ").FontSize(7).FontColor(TextMuted);
                            }
                            if (!string.IsNullOrWhiteSpace(settings.Address))
                                r.AutoItem().Text(settings.Address).FontSize(7).FontColor(TextMuted);
                        });
                    });

                    hdr.Item().Height(3).Background(Green);
                });

                // ── CONTENIDO ────────────────────────────────────────────────
                page.Content().PaddingHorizontal(12).PaddingTop(12).Column(col =>
                {
                    // ── Tarjeta de orden ─────────────────────────────────────
                    col.Item().Background(CardBg).Padding(12).Column(c =>
                    {
                        // Número de orden + badge PAGADO
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text("COMPROBANTE DE PAGO")
                                    .FontSize(6).SemiBold().FontColor(Purple);

                                inner.Item().PaddingTop(4).Text(invoice.NumeroOrden)
                                    .FontSize(15).ExtraBold().FontColor(Orange);

                                inner.Item().PaddingTop(3)
                                    .Text($"{invoice.SaleDate:dd/MM/yyyy  HH:mm}")
                                    .FontSize(7).FontColor(TextMuted);
                            });

                            r.ConstantItem(54).AlignRight().AlignMiddle()
                                .Background(Green)
                                .PaddingHorizontal(6).PaddingVertical(4)
                                .AlignCenter().Text("✓ PAGADO")
                                .FontSize(7).Bold().FontColor(White);
                        });

                        // Cliente
                        c.Item().PaddingTop(10).BorderTop(0.5f).BorderColor(CardLine).PaddingTop(8).Row(r =>
                        {
                            r.ConstantItem(52).Text("Cliente:").FontSize(7.5f).FontColor(TextMuted);
                            r.RelativeItem().Text(invoice.CustomerName)
                                .FontSize(7.5f).SemiBold().FontColor(White);
                        });

                        // Nota (orden web)
                        if (!string.IsNullOrWhiteSpace(invoice.Nota))
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(52).Text("Orden:").FontSize(7.5f).FontColor(TextMuted);
                                r.RelativeItem().Text(invoice.Nota)
                                    .FontSize(7.5f).SemiBold().FontColor(Purple);
                            });
                    });

                    // ── Divisor ──────────────────────────────────────────────
                    col.Item().PaddingTop(10).Height(2).Background(Purple);

                    // ── Encabezado tabla ─────────────────────────────────────
                    col.Item().Background(DeepBg).PaddingHorizontal(8).PaddingVertical(6).Row(r =>
                    {
                        r.ConstantItem(22).Text("CANT").FontSize(7).Bold().FontColor(White);
                        r.RelativeItem().Text("DESCRIPCIÓN").FontSize(7).Bold().FontColor(White);
                        r.ConstantItem(56).AlignRight().Text("PRECIO").FontSize(7).Bold().FontColor(White);
                    });

                    // ── Filas de productos ───────────────────────────────────
                    bool alt = false;
                    foreach (var detail in invoice.Details)
                    {
                        string bg = alt ? LightBg : White;
                        col.Item().Background(bg).BorderBottom(0.5f).BorderColor(LineColor)
                            .PaddingHorizontal(8).PaddingVertical(6)
                            .Row(r =>
                            {
                                r.ConstantItem(22).AlignMiddle()
                                    .Text(detail.Quantity.ToString()).FontSize(8.5f).FontColor(TextSub);

                                r.RelativeItem().AlignMiddle().Column(c =>
                                {
                                    c.Item().Text(detail.Concept).FontSize(8.5f).SemiBold();
                                    if (!string.IsNullOrWhiteSpace(detail.Nota))
                                        c.Item().PaddingTop(2).Text($"※ {detail.Nota}")
                                            .FontSize(7).Italic().FontColor(TextMuted);
                                });

                                r.ConstantItem(56).AlignRight().AlignMiddle()
                                    .Text($"C$ {detail.SubTotal:N2}").FontSize(8.5f).SemiBold();
                            });
                        alt = !alt;
                    }

                    // ── Barra cierre tabla ───────────────────────────────────
                    col.Item().Height(2).Background(Green);

                    // ── Descuento (solo si aplica) ───────────────────────────
                    if (invoice.DiscountAmount > 0)
                    {
                        col.Item().PaddingTop(8).PaddingHorizontal(12).Column(c =>
                        {
                            c.Item().AlignRight().Row(r =>
                            {
                                r.AutoItem().Text("Subtotal:").FontSize(8).FontColor(TextSub);
                                r.ConstantItem(72).AlignRight()
                                    .Text($"C$ {subtotal:N2}").FontSize(8).FontColor(TextSub);
                            });
                            c.Item().PaddingTop(4).AlignRight().Row(r =>
                            {
                                r.AutoItem().Text("Descuento:").FontSize(8).FontColor(Orange);
                                r.ConstantItem(72).AlignRight()
                                    .Text($"-C$ {invoice.DiscountAmount:N2}").FontSize(8).SemiBold().FontColor(Orange);
                            });
                        });
                    }

                    // ── Bloque TOTAL ─────────────────────────────────────────
                    col.Item().PaddingTop(invoice.DiscountAmount > 0 ? 8 : 10)
                        .Background(CardBg).PaddingHorizontal(12).PaddingVertical(10).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL").FontSize(14).Bold().FontColor(White);
                            r.AutoItem().Text($"C$ {invoice.TotalAmount:N2}")
                                .FontSize(16).ExtraBold().FontColor(Orange);
                        });

                    // ── Monto recibido (sin cambio) ──────────────────────────
                    if (invoice.AmountReceived.HasValue)
                    {
                        col.Item().PaddingTop(8).PaddingHorizontal(12).AlignRight().Row(r =>
                        {
                            r.AutoItem().Text("Recibido:").FontSize(8).FontColor(TextSub);
                            r.ConstantItem(72).AlignRight()
                                .Text($"C$ {invoice.AmountReceived:N2}").FontSize(8).FontColor(TextSub);
                        });
                    }

                    // ── Sección de pago ──────────────────────────────────────
                    col.Item().PaddingTop(10).Height(2).Background(Purple);

                    col.Item().Background(CardBg).PaddingHorizontal(12).PaddingVertical(10).Column(c =>
                    {
                        c.Item().Text("DATOS DE PAGO").FontSize(6.5f).SemiBold().FontColor(Purple);

                        c.Item().PaddingTop(8).Column(inner =>
                        {
                            // Método de pago
                            if (!string.IsNullOrWhiteSpace(invoice.PaymentMethod))
                                inner.Item().PaddingBottom(5).Row(r =>
                                {
                                    r.ConstantItem(70).Text("Método:").FontSize(7.5f).FontColor(TextMuted);
                                    r.RelativeItem().Text(invoice.PaymentMethod)
                                        .FontSize(7.5f).SemiBold().FontColor(White);
                                });

                            // Banco
                            if (!string.IsNullOrWhiteSpace(invoice.MetodoPagoBanco))
                                inner.Item().PaddingBottom(5).Row(r =>
                                {
                                    r.ConstantItem(70).Text("Banco:").FontSize(7.5f).FontColor(TextMuted);
                                    r.RelativeItem().Text(invoice.MetodoPagoBanco)
                                        .FontSize(7.5f).SemiBold().FontColor(White);
                                });

                            // Tipo de cuenta
                            if (!string.IsNullOrWhiteSpace(invoice.MetodoPagoTipoCuenta))
                                inner.Item().PaddingBottom(5).Row(r =>
                                {
                                    r.ConstantItem(70).Text("Tipo cuenta:").FontSize(7.5f).FontColor(TextMuted);
                                    r.RelativeItem().Text(invoice.MetodoPagoTipoCuenta)
                                        .FontSize(7.5f).SemiBold().FontColor(White);
                                });

                            // Número de cuenta
                            if (!string.IsNullOrWhiteSpace(invoice.MetodoPagoNumeroCuenta))
                                inner.Item().PaddingBottom(5).Row(r =>
                                {
                                    r.ConstantItem(70).Text("N° Cuenta:").FontSize(7.5f).FontColor(TextMuted);
                                    r.RelativeItem().Text(invoice.MetodoPagoNumeroCuenta)
                                        .FontSize(7.5f).ExtraBold().FontColor(Green);
                                });

                            // Titular
                            if (!string.IsNullOrWhiteSpace(invoice.MetodoPagoTitular))
                                inner.Item().PaddingBottom(5).Row(r =>
                                {
                                    r.ConstantItem(70).Text("Titular:").FontSize(7.5f).FontColor(TextMuted);
                                    r.RelativeItem().Text(invoice.MetodoPagoTitular)
                                        .FontSize(7.5f).SemiBold().FontColor(White);
                                });

                            // Referencia de pago
                            if (!string.IsNullOrWhiteSpace(invoice.PaymentReference))
                            {
                                inner.Item().PaddingTop(4).BorderTop(0.5f).BorderColor(CardLine).PaddingTop(8).Row(r =>
                                {
                                    r.ConstantItem(70).Text("Referencia:").FontSize(8).SemiBold().FontColor(TextMuted);
                                    r.RelativeItem().Text(invoice.PaymentReference)
                                        .FontSize(8.5f).ExtraBold().FontColor(Orange);
                                });
                            }
                        });
                    });

                    col.Item().PaddingTop(12);
                });

                // ── PIE DE PÁGINA ─────────────────────────────────────────────
                page.Footer().Column(f =>
                {
                    f.Item().Height(3).Background(Green);
                    f.Item().Background(DeepBg).PaddingVertical(12).Column(c =>
                    {
                        c.Item().AlignCenter().Text("¡GRACIAS POR SU PREFERENCIA!")
                            .FontSize(9).Bold().FontColor(Purple);
                        c.Item().PaddingTop(3).AlignCenter().Text(settings.BusinessName)
                            .FontSize(7).FontColor(TextMuted);
                        c.Item().PaddingTop(5).AlignCenter().Text("Documento sin valor legal")
                            .FontSize(6).Italic().FontColor(TextMuted);
                    });
                });
            });
        }).GeneratePdf();

        return new PdfTicketResponseDto
        {
            Content     = pdfData,
            OrderNumber = invoice.NumeroOrden
        };
    }

    // ── IMAGEN PNG ──────────────────────────────────────────────────────────
    public async Task<PdfTicketResponseDto> GenerateTicketImageAsync(Guid invoiceId)
    {
        var pdf = await GenerateTicketAsync(invoiceId);

        using var ms     = new MemoryStream(pdf.Content);
        using var bitmap = Conversion.ToImage(ms, leaveOpen: false, page: 0,
            options: new RenderOptions(Dpi: 150));

        var trimmed = TrimBottomWhitespace(bitmap);

        using var image   = SKImage.FromBitmap(trimmed);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 95);

        var bytes = encoded.ToArray();
        SaveLocalCopy(bytes, pdf.OrderNumber);

        return new PdfTicketResponseDto
        {
            Content     = bytes,
            OrderNumber = pdf.OrderNumber
        };
    }

    // ── HELPERS ─────────────────────────────────────────────────────────────

    private static SKBitmap TrimBottomWhitespace(SKBitmap bitmap)
    {
        int lastContent = 0;
        for (int y = bitmap.Height - 1; y >= 0; y--)
        {
            bool allWhite = true;
            for (int x = 0; x < bitmap.Width; x++)
            {
                var px = bitmap.GetPixel(x, y);
                if (px.Red < 250 || px.Green < 250 || px.Blue < 250)
                {
                    allWhite = false;
                    break;
                }
            }
            if (!allWhite) { lastContent = y; break; }
        }

        int newHeight = Math.Min(lastContent + 24, bitmap.Height);
        if (newHeight >= bitmap.Height) return bitmap;

        var cropped = new SKBitmap(bitmap.Width, newHeight);
        using var canvas = new SKCanvas(cropped);
        canvas.DrawBitmap(bitmap, 0, 0);
        return cropped;
    }

    private static void SaveLocalCopy(byte[] imageBytes, string orderNumber)
    {
        try
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "facturas");
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, $"{orderNumber}.png"), imageBytes);
        }
        catch { }
    }
}
