namespace MaxPlus.IPTV.Core.Entities;

/// <summary>
/// Fila plana devuelta por sp_IptvAccounts_ObtenerConClientes.
/// El servicio la agrupa por AccountId para construir la vista jerarquizada.
/// </summary>
public class IptvAccountSlotRow
{
    // ── Datos de la cuenta ──────────────────────────────────
    public Guid     AccountId      { get; set; }
    public string   AccessUser     { get; set; } = string.Empty;
    public string   AccessPassword { get; set; } = string.Empty;
    public string?  PlatformUrl    { get; set; }
    public string?  PinCode        { get; set; }
    public Guid?    TipoServicioId        { get; set; }
    public string?  ServiceName           { get; set; }
    public int       MaxSlots              { get; set; }
    public decimal   PurchasePrice         { get; set; }
    public DateTime? AccountExpirationDate { get; set; }
    public int?      AccountDaysRemaining  { get; set; }
    public string?   Notes                 { get; set; }
    public DateTime  CreatedAt             { get; set; }

    // ── Datos del slot / cliente (NULL si no hay cliente) ───
    public Guid?     SubscriptionId { get; set; }
    public Guid?     CustomerId     { get; set; }
    public string?   CustomerName   { get; set; }
    public string?   CustomerPhone  { get; set; }
    public string?   CustomerEmail  { get; set; }
    /// <summary>Usuario de perfil asignado a este slot (Netflix/Streaming).</summary>
    public string?   ProfileUser    { get; set; }
    /// <summary>PIN de perfil asignado a este slot (Netflix/Streaming).</summary>
    public string?   ProfilePin     { get; set; }
    public DateTime? StartDate      { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string?   SlotStatus     { get; set; }
    public int?      DaysRemaining  { get; set; }
}
