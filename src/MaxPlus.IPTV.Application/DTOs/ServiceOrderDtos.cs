using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

public class ServiceOrderCreateDto
{
    [Required(ErrorMessage = "El nombre del cliente es requerido.")]
    [MaxLength(150)]
    public string  CustomerName   { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CustomerPhone  { get; set; }

    [MaxLength(100)]
    public string? CustomerEmail  { get; set; }

    public Guid?   TipoServicioId { get; set; }

    [MaxLength(500)]
    public string? Notes          { get; set; }
}

public class ServiceOrderApproveDto
{
    // Opcional: si se envía, usa ese cliente existente.
    // Si no, el sistema busca por email/teléfono y crea uno si no existe.
    public Guid? CustomerId { get; set; }

    // Cuenta IPTV del proveedor desde la cual se asigna el slot
    [Required]
    public Guid IptvAccountId { get; set; }

    // Opcional: si no se envía, se calcula desde TipoServicioId de la orden
    public DateTime? ExpirationDate { get; set; }

    // Credenciales de perfil (solo Netflix/Streaming)
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
}
