using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

// Item individual del carrito
public class ServiceOrderItemCreateDto
{
    [Required]
    public Guid TipoServicioId { get; set; }

    [Range(1, 24, ErrorMessage = "DurationMonths debe estar entre 1 y 24.")]
    public int  DurationMonths { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "Cantidad debe estar entre 1 y 10.")]
    public int  Cantidad       { get; set; } = 1;
}

public class ServiceOrderCreateDto
{
    [Required(ErrorMessage = "El nombre del cliente es requerido.")]
    [MaxLength(150)]
    public string  CustomerName   { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CustomerPhone  { get; set; }

    [MaxLength(100)]
    public string? CustomerEmail  { get; set; }

    // Orden simple (un solo servicio — backward compatible)
    public Guid?   TipoServicioId { get; set; }

    // Carrito con múltiples servicios y duraciones distintas
    public List<ServiceOrderItemCreateDto> Items { get; set; } = [];

    [MaxLength(500)]
    public string? Notes          { get; set; }
}

// Aprobación de un item individual del carrito
public class ServiceOrderItemApproveDto
{
    [Required]
    public Guid      ServiceOrderItemId { get; set; }

    [Required]
    public Guid      IptvAccountId      { get; set; }

    public DateTime? ExpirationDate     { get; set; }
    public string?   ProfileUser        { get; set; }
    public string?   ProfilePin         { get; set; }
}

public class ServiceOrderApproveDto
{
    // Opcional: si se envía, usa ese cliente existente.
    // Si no, el sistema busca por email/teléfono y crea uno si no existe.
    public Guid? CustomerId { get; set; }

    // Para órdenes simples (un solo servicio — backward compatible)
    public Guid IptvAccountId { get; set; }

    // Para órdenes de carrito (múltiples servicios)
    public List<ServiceOrderItemApproveDto> Items { get; set; } = [];

    // Opcional: si no se envía, se calcula desde TipoServicioId de la orden
    public DateTime? ExpirationDate { get; set; }

    // Credenciales de perfil (solo Netflix/Streaming — orden simple)
    public string? ProfileUser { get; set; }
    public string? ProfilePin  { get; set; }

    // Pago — genera la factura con estos datos
    public string?  PaymentMethod    { get; set; } = "Orden Digital";
    public Guid?    MetodoPagoId     { get; set; }
    public string?  PaymentReference { get; set; }
    public decimal? AmountReceived   { get; set; }
    public decimal  DiscountAmount   { get; set; } = 0;
}

public class ServiceOrderRejectDto
{
    [MaxLength(255)]
    public string? Reason { get; set; }
}

public class ServiceOrderItemResponseDto
{
    public Guid     Id             { get; set; }
    public Guid     TipoServicioId { get; set; }
    public string?  ServiceName    { get; set; }
    public string?  Plataforma     { get; set; }
    public string?  ImageUrl       { get; set; }
    public int      DurationMonths { get; set; }
    public int      Cantidad       { get; set; }
    public decimal  UnitPrice      { get; set; }
    public decimal  SubTotal       { get; set; }
    public Guid?    SubscriptionId { get; set; }
}

public class ServiceOrderResponseDto
{
    public Guid      Id              { get; set; }
    public string    NumeroOrden     { get; set; } = string.Empty;
    public string    CustomerName    { get; set; } = string.Empty;
    public string?   CustomerPhone   { get; set; }
    public string?   CustomerEmail   { get; set; }
    public Guid?     TipoServicioId  { get; set; }
    public string?   ServiceName     { get; set; }
    public string?   Notes           { get; set; }
    public string    Status          { get; set; } = string.Empty;
    public Guid?     ApprovedBy      { get; set; }
    public DateTime? ApprovedAt      { get; set; }
    public Guid?     SubscriptionId  { get; set; }
    public string?   RejectionReason { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public int       ItemCount       { get; set; }
    public List<ServiceOrderItemResponseDto> Items { get; set; } = [];
}
