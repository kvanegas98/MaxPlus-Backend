namespace MaxPlus.IPTV.Core.Entities;

/// <summary>
/// Cuenta IPTV comprada al proveedor.
/// Se registra usuario/contraseña + precio pagado.
/// Se puede asignar a hasta MaxSlots clientes.
/// </summary>
public class IptvAccount
{
    public Guid   Id             { get; set; }
    public string  AccessUser     { get; set; } = string.Empty;
    public string  AccessPassword { get; set; } = string.Empty;
    /// <summary>Correo de la cuenta. Solo aplica para Netflix y Streaming.</summary>
    public string? AccessEmail    { get; set; }
    public string? PlatformUrl    { get; set; }
    public string? PinCode        { get; set; }

    /// <summary>Tipo de servicio al que pertenece esta cuenta (ej: Netflix, IPTV).</summary>
    public Guid?   TipoServicioId { get; set; }
    public string? ServiceName    { get; set; }

    /// <summary>Máximo de clientes que pueden compartir esta cuenta (ej: 3).</summary>
    public int MaxSlots { get; set; } = 1;

    /// <summary>Precio que pagó el admin al proveedor.</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>Fecha en que vence la cuenta con el proveedor.</summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Días restantes para que venza la cuenta (calculado en query).</summary>
    public int? DaysRemaining { get; set; }

    public string?   Notes    { get; set; }
    public bool      IsActive { get; set; } = true;
    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Slots usados actualmente (calculado en query).</summary>
    public int UsedSlots { get; set; }
}
