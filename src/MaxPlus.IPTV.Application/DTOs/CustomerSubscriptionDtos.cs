using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

// ── Suscripciones de Clientes ────────────────────────────────────────────────

public class CustomerSubscriptionCreateDto
{
    // Cliente — todos opcionales: si no se envía nada, Status = Unassigned
    public Guid?   CustomerId    { get; set; }
    [MaxLength(150)]
    public string? CustomerName  { get; set; }
    [MaxLength(20)]
    public string? CustomerPhone { get; set; }
    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    // ── Suscripción ──────────────────────────────────────────────────────────
    public Guid?   TipoServicioId   { get; set; }
    public string  SubscriptionType { get; set; } = "Paid";

    [MaxLength(255)]
    public string? PlatformUrl    { get; set; }
    [MaxLength(100)]
    public string? AccessUser     { get; set; }
    [MaxLength(100)]
    public string? AccessPassword { get; set; }
    [MaxLength(20)]
    public string? PinCode        { get; set; }

    /// Opcional: si no se envía se calcula con TipoServicio.DurationDays (default 30 días)
    public DateTime? ExpirationDate { get; set; }

    // ── Pago (opcional — si se envía, genera factura automáticamente) ────────
    public string?  PaymentMethod    { get; set; }
    public Guid?    MetodoPagoId     { get; set; }
    public string?  PaymentReference { get; set; }
    public decimal? AmountReceived   { get; set; }
    public decimal  DiscountAmount   { get; set; } = 0;
}

/// DTO para asignar un cliente a una suscripción Unassigned
public class AssignCustomerDto
{
    // Cliente existente o nuevo
    public Guid?   CustomerId    { get; set; }
    [MaxLength(150)]
    public string? CustomerName  { get; set; }
    [MaxLength(20)]
    public string? CustomerPhone { get; set; }
    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    // Pago opcional → genera factura
    public string?  PaymentMethod    { get; set; }
    public Guid?    MetodoPagoId     { get; set; }
    public string?  PaymentReference { get; set; }
    public decimal? AmountReceived   { get; set; }
    public decimal  DiscountAmount   { get; set; } = 0;
}

public class CustomerSubscriptionUpdateDto
{
    [MaxLength(255)]
    public string? PlatformUrl    { get; set; }
    [MaxLength(100)]
    public string? AccessUser     { get; set; }
    [MaxLength(100)]
    public string? AccessPassword { get; set; }
    [MaxLength(20)]
    public string? PinCode        { get; set; }

    [Required]
    public DateTime ExpirationDate { get; set; }

    [Required]
    public string Status { get; set; } = "Active";
}

public class CustomerSubscriptionResponseDto
{
    public Guid     Id               { get; set; }
    public Guid?    CustomerId       { get; set; }
    public string?  CustomerName     { get; set; }
    public Guid?    TipoServicioId   { get; set; }
    public string?  ServiceName      { get; set; }
    public string   SubscriptionType { get; set; } = string.Empty;
    public string?  PlatformUrl      { get; set; }
    public string?  AccessUser       { get; set; }
    public string?  AccessPassword   { get; set; }
    public string?  PinCode          { get; set; }
    public DateTime StartDate        { get; set; }
    public DateTime ExpirationDate   { get; set; }
    public string   Status           { get; set; } = string.Empty;
    public bool     IsExpired        => ExpirationDate < DateTime.UtcNow;
    public DateTime CreatedAt        { get; set; }
    public Guid?    InvoiceId        { get; set; }
}
