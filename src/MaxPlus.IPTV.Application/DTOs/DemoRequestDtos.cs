using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

// ── Demo Requests ────────────────────────────────────────────────────────────

public class DemoRequestCreateDto
{
    [Required(ErrorMessage = "El nombre del cliente es requerido.")]
    [MaxLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    public Guid? CustomerId { get; set; }
    public Guid? TipoServicioId { get; set; }
}

public class DemoApproveDto
{
    public string? DemoUrl { get; set; }
    public string? ResponseHtml { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class DemoRejectDto
{
    [MaxLength(255)]
    public string? Reason { get; set; }
}

public class DemoVerifyDto
{
    [Required]
    [MaxLength(6)]
    public string Code { get; set; } = string.Empty;
}

public class DemoRequestResponseDto
{
    public Guid     Id              { get; set; }
    public string   CustomerName    { get; set; } = string.Empty;
    public string?  CustomerPhone   { get; set; }
    public string?  CustomerEmail   { get; set; }
    public Guid?    CustomerId      { get; set; }
    public Guid?    TipoServicioId  { get; set; }
    
    public string?  IpAddress       { get; set; }
    public string?  Country         { get; set; }
    public bool     IsPhoneVerified { get; set; }
    
    public string?  ServiceName     { get; set; }
    public string   Status          { get; set; } = string.Empty;
    public string?  DemoUrl         { get; set; }
    public string?  ResponseHtml    { get; set; }
    public Guid?    ApprovedBy      { get; set; }
    public DateTime? ApprovedAt     { get; set; }
    public DateTime? ExpiresAt      { get; set; }
    public string?  RejectionReason { get; set; }
    public DateTime CreatedAt       { get; set; }
}
