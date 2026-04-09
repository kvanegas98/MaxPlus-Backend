using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

public class InvoiceCreateDto
{
    [MaxLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    public string   CustomerName    { get; set; } = string.Empty;

    public Guid     UserId          { get; set; }
    public Guid?    CustomerId      { get; set; }

    [Required(ErrorMessage = "El tipo de orden es requerido.")]
    public string   OrderType       { get; set; } = "Venta";

    public string?  PaymentMethod    { get; set; }
    public Guid?    MetodoPagoId     { get; set; }
    public string?  PaymentReference { get; set; }
    public decimal? AmountReceived   { get; set; }

    public string? Nota { get; set; }

    public Guid?   SubscriptionId  { get; set; }

    [Required(ErrorMessage = "La factura debe tener detalle.")]
    [MinLength(1, ErrorMessage = "Debe agregar al menos un servicio a la factura.")]
    public List<InvoiceDetailCreateDto> Details { get; set; } = new();
}

public class InvoiceDetailCreateDto
{
    public Guid?   TipoServicioId { get; set; }

    // Suscripción Unassigned a activar con esta línea de factura
    public Guid?   SubscriptionId { get; set; }

    [Required(ErrorMessage = "El concepto es requerido.")]
    [MaxLength(255, ErrorMessage = "El concepto no puede exceder los 255 caracteres.")]
    public string  Concept        { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int     Quantity        { get; set; }

    [Range(1, 24, ErrorMessage = "DurationMonths debe estar entre 1 y 24.")]
    public int     DurationMonths  { get; set; } = 1;

    public decimal UnitPrice      { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Nota { get; set; }
}

public class InvoiceResponseDto
{
    public Guid     Id              { get; set; }
    public string   NumeroOrden     { get; set; } = string.Empty;
    public DateTime SaleDate        { get; set; }
    public string   CustomerName    { get; set; } = string.Empty;
    public Guid?    CustomerId      { get; set; }
    public string   OrderType       { get; set; } = string.Empty;
    public string?  PaymentMethod    { get; set; }
    public Guid?    MetodoPagoId     { get; set; }
    public string?  PaymentReference { get; set; }
    public decimal? AmountReceived   { get; set; }
    public decimal  DiscountAmount   { get; set; }
    public decimal  TotalAmount      { get; set; }
    public string   Status           { get; set; } = string.Empty;
    public string?  Nota             { get; set; }
    public Guid?    SubscriptionId   { get; set; }
    public decimal  Change          => PaymentMethod?.StartsWith("Efectivo") == true && AmountReceived.HasValue
                                        ? AmountReceived.Value - TotalAmount
                                        : 0;
    public List<InvoiceDetailResponseDto> Details { get; set; } = new();
}

public class InvoiceDetailResponseDto
{
    public string  Concept        { get; set; } = string.Empty;
    public int     Quantity       { get; set; }
    public decimal UnitPrice      { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubTotal       { get; set; }
    public string? Nota           { get; set; }
}
