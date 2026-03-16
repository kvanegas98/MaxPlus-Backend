namespace MaxPlus.IPTV.Core.Entities;

/// <summary>
/// Tipo de servicio: catálogo configurable de servicios IPTV con precio.
/// Ej: "Suscripción Premium 1 Mes", "Demo 24h", "Pantalla Adicional"
/// </summary>
public class ServiceType
{
    public Guid     Id           { get; set; }
    public string   Name         { get; set; } = string.Empty;
    public string?  Description  { get; set; }
    public decimal  Price        { get; set; }
    public decimal  PurchasePrice { get; set; }
    public int      DurationDays { get; set; } = 30;
    public string   Category     { get; set; } = "Paid";  // Paid | Demo
    public string   Plataforma   { get; set; } = "IPTV"; // IPTV | FlujoTV | Netflix | Streaming
    public string?  ImageUrl     { get; set; }
    public bool     IsActive     { get; set; } = true;
    public DateTime CreatedAt    { get; set; }
}
