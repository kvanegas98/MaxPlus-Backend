using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

// ── CREATE ──────────────────────────────────────────────────────────────────

public class IptvAccountCreateDto
{
    [Required, MaxLength(100)]
    public string AccessUser { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string AccessPassword { get; set; } = string.Empty;

    /// <summary>Correo de la cuenta. Requerido para Netflix y Streaming.</summary>
    [MaxLength(150)]
    public string? AccessEmail { get; set; }

    [MaxLength(255)]
    public string? PlatformUrl { get; set; }

    [MaxLength(20)]
    public string? PinCode { get; set; }

    /// <summary>Tipo de servicio al que pertenece esta cuenta (ej: Netflix, IPTV).</summary>
    public Guid? TipoServicioId { get; set; }

    [Range(1, 10)]
    public int MaxSlots { get; set; } = 1;

    [Range(0, double.MaxValue)]
    public decimal PurchasePrice { get; set; } = 0;

    /// <summary>Fecha en que vence la cuenta con el proveedor.</summary>
    public DateTime? ExpirationDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

// ── UPDATE ───────────────────────────────────────────────────────────────────

public class IptvAccountUpdateDto
{
    [Required, MaxLength(100)]
    public string AccessUser { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string AccessPassword { get; set; } = string.Empty;

    /// <summary>Correo de la cuenta. Requerido para Netflix y Streaming.</summary>
    [MaxLength(150)]
    public string? AccessEmail { get; set; }

    [MaxLength(255)]
    public string? PlatformUrl { get; set; }

    [MaxLength(20)]
    public string? PinCode { get; set; }

    /// <summary>Tipo de servicio al que pertenece esta cuenta (ej: Netflix, IPTV).</summary>
    public Guid? TipoServicioId { get; set; }

    [Range(1, 10)]
    public int MaxSlots { get; set; } = 1;

    [Range(0, double.MaxValue)]
    public decimal PurchasePrice { get; set; }

    /// <summary>Fecha en que vence la cuenta con el proveedor.</summary>
    public DateTime? ExpirationDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

// ── ASSIGN CLIENT ────────────────────────────────────────────────────────────

public class IptvAccountAssignClientDto
{
    // Cliente existente o nuevo
    public Guid? CustomerId { get; set; }

    [MaxLength(150)]
    public string? CustomerName { get; set; }

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    // Tipo de servicio que se vende al cliente
    // → determina precio de venta y duración automáticamente
    public Guid? TipoServicioId { get; set; }

    // Fecha de vencimiento: si null se calcula con TipoServicio.DurationDays (o 30 días)
    public DateTime? ExpirationDate { get; set; }

    // Credenciales de perfil (solo Netflix/Streaming)
    // → el admin las ingresa al aprobar la orden; se guardan en la suscripción
    public string? ProfileUser { get; set; }
    public string? ProfilePin  { get; set; }

    // Pago opcional → genera factura
    public string?  PaymentMethod    { get; set; }
    public Guid?    MetodoPagoId     { get; set; }
    public string?  PaymentReference { get; set; }
    public decimal? AmountReceived   { get; set; }
    public decimal  DiscountAmount   { get; set; } = 0;
}

// ── SLOT RESPONSE ────────────────────────────────────────────────────────────

public class IptvAccountSlotDto
{
    public Guid?     SubscriptionId { get; set; }
    public Guid?     CustomerId     { get; set; }
    public string?   CustomerName   { get; set; }
    public string?   CustomerPhone  { get; set; }
    public string?   CustomerEmail  { get; set; }
    /// <summary>Usuario de perfil (Netflix/Streaming). Null para IPTV/FlujoTV.</summary>
    public string?   ProfileUser    { get; set; }
    /// <summary>PIN de perfil (Netflix/Streaming). Null para IPTV/FlujoTV.</summary>
    public string?   ProfilePin     { get; set; }
    public DateTime? StartDate      { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int?      DaysRemaining  { get; set; }

    /// <summary>Active | Expired | Available</summary>
    public string Status { get; set; } = "Available";
}

// ── ACCOUNT RESPONSE ─────────────────────────────────────────────────────────

public class IptvAccountResponseDto
{
    public Guid     Id             { get; set; }
    public string   AccessUser     { get; set; } = string.Empty;
    public string   AccessPassword { get; set; } = string.Empty;
    /// <summary>Correo de la cuenta. Solo aplica para Netflix y Streaming.</summary>
    public string?  AccessEmail    { get; set; }
    public string?  PlatformUrl    { get; set; }
    public string?  PinCode        { get; set; }
    public Guid?    TipoServicioId { get; set; }
    public string?  ServiceName    { get; set; }
    public int       MaxSlots       { get; set; }
    public decimal   PurchasePrice  { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int?      DaysRemaining  { get; set; }
    public string?   Notes          { get; set; }
    public bool      IsActive       { get; set; }
    public DateTime  CreatedAt      { get; set; }

    public int UsedSlots      { get; set; }
    public int AvailableSlots { get; set; }

    /// <summary>Slots con clientes + slots vacíos hasta MaxSlots.</summary>
    public List<IptvAccountSlotDto> Clients { get; set; } = [];
}

// ── AVAILABLE (dropdown selector) ────────────────────────────────────────────

public class IptvAccountAvailableDto
{
    public Guid      Id             { get; set; }
    public string    AccessUser     { get; set; } = string.Empty;
    public string?   ServiceName    { get; set; }
    public int       MaxSlots       { get; set; }
    public int       UsedSlots      { get; set; }
    public int       AvailableSlots { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int?      DaysRemaining  { get; set; }
}

// ── STATS ─────────────────────────────────────────────────────────────────────

public class IptvAccountStatsDto
{
    public int TotalAccounts    { get; set; }
    public int TotalSlots       { get; set; }
    public int UsedSlots        { get; set; }
    public int AvailableSlots   { get; set; }
}

// ── ASSIGN RESPONSE ──────────────────────────────────────────────────────────

public class IptvAccountAssignResponseDto
{
    public Guid     SubscriptionId { get; set; }
    public Guid     AccountId      { get; set; }
    public string   CustomerName   { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public Guid?    InvoiceId      { get; set; }
    public string   Message        { get; set; } = string.Empty;
}
