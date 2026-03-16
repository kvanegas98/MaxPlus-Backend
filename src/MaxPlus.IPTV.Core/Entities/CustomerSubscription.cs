namespace MaxPlus.IPTV.Core.Entities;

/// <summary>
/// Suscripción vendida a un cliente. Vincula cliente ↔ cuenta proveedor.
/// CustomerId es nullable: una suscripción puede crearse sin cliente (Status = Unassigned)
/// y asignarse después.
/// </summary>
public class CustomerSubscription
{
    public Guid  Id         { get; set; }
    public Guid? CustomerId { get; set; }

    /// <summary>Cuenta IPTV padre (modelo nuevo). Null en suscripciones antiguas.</summary>
    public Guid? IptvAccountId { get; set; }

    public Guid? TipoServicioId { get; set; }

    public string SubscriptionType { get; set; } = "Paid";  // Paid | Demo

    public string? PlatformUrl    { get; set; }
    public string? AccessUser     { get; set; }
    public string? AccessPassword { get; set; }
    public string? PinCode        { get; set; }
    public string? ProfileUser    { get; set; }
    public string? ProfilePin     { get; set; }

    public DateTime StartDate      { get; set; }
    public DateTime ExpirationDate { get; set; }

    /// <summary>Active | Unassigned | Expired | Cancelled | Renewed</summary>
    public string Status { get; set; } = "Active";

    // Datos JOIN
    public string? ServiceName      { get; set; }
    public string? CustomerName     { get; set; }
    public string? CustomerEmail    { get; set; }
    public string? CustomerPhone    { get; set; }
    public string? ProviderUsername { get; set; }
    public string? ProviderHostUrl  { get; set; }

    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
