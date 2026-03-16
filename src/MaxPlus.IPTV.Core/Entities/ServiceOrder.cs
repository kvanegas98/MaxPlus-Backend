namespace MaxPlus.IPTV.Core.Entities;

public class ServiceOrder
{
    public Guid     Id              { get; set; }
    public string   NumeroOrden     { get; set; } = string.Empty;
    public string   CustomerName    { get; set; } = string.Empty;
    public string?  CustomerPhone   { get; set; }
    public string?  CustomerEmail   { get; set; }
    public Guid?    TipoServicioId  { get; set; }
    public string?  ServiceName     { get; set; } // JOIN
    public string?  Notes           { get; set; }
    public string   Status          { get; set; } = "Pending"; // Pending | Approved | Rejected
    public Guid?    ApprovedBy      { get; set; }
    public DateTime? ApprovedAt     { get; set; }
    public Guid?    SubscriptionId  { get; set; }
    public string?  RejectionReason { get; set; }
    public string?  IpAddress       { get; set; }
    public DateTime CreatedAt       { get; set; }
}
