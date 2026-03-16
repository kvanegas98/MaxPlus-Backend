namespace MaxPlus.IPTV.Core.Entities;

/// <summary>
/// Solicitud de demo IPTV gratuita.
/// El cliente la solicita desde el menú digital; el admin la aprueba/rechaza.
/// </summary>
public class DemoRequest
{
    public Guid     Id              { get; set; }
    public string   CustomerName    { get; set; } = string.Empty;
    public string?  CustomerPhone   { get; set; }
    public string?  CustomerEmail   { get; set; }
    public Guid?    CustomerId      { get; set; }
    public Guid?    TipoServicioId  { get; set; }

    public string?  IpAddress             { get; set; }
    public string?  Country               { get; set; }
    public string?  PhoneVerificationCode { get; set; }
    public bool     IsPhoneVerified       { get; set; }

    public string   Status          { get; set; } = "Pending";  // Pending | Approved | Rejected | Expired
    public string?  DemoUrl         { get; set; }
    public string?  ResponseHtml    { get; set; }
    public Guid?    ApprovedBy      { get; set; }
    public DateTime? ApprovedAt     { get; set; }
    public DateTime? ExpiresAt      { get; set; }
    public string?  RejectionReason { get; set; }

    // Datos JOIN
    public string?  ServiceName     { get; set; }

    public DateTime CreatedAt       { get; set; }
}
