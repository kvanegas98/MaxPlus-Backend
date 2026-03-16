using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

public class RenewalCreateDto
{
    [Required(ErrorMessage = "La nueva fecha de expiración es requerida.")]
    public DateTime NewExpiration { get; set; }

    // Pago — si se envía, genera factura automáticamente
    public string?  PaymentMethod    { get; set; }
    public Guid?    MetodoPagoId     { get; set; }
    public string?  PaymentReference { get; set; }
    public decimal? AmountReceived   { get; set; }
    public decimal  DiscountAmount   { get; set; } = 0;
}

public class RenewalResponseDto
{
    public Guid     NewSubscriptionId { get; set; }
    public Guid     OldSubscriptionId { get; set; }
    public DateTime NewExpiration     { get; set; }
    public Guid?    InvoiceId         { get; set; }
    public string   Message           { get; set; } = string.Empty;
}
