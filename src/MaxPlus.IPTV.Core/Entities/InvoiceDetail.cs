namespace MaxPlus.IPTV.Core.Entities;

public class InvoiceDetail
{
    public Guid    Id              { get; set; }
    public Guid    InvoiceId       { get; set; }
    public Guid?   TipoServicioId  { get; set; }  // FK opcional al catálogo de servicios
    public Guid?   SubscriptionId  { get; set; }  // Suscripción Unassigned a activar con esta línea
    public string  Concept         { get; set; } = string.Empty;
    public int     Quantity        { get; set; }
    public int     DurationMonths  { get; set; } = 1;  // Meses pagados — para calcular vencimiento al activar suscripción
    public decimal UnitPrice       { get; set; }
    public decimal DiscountAmount  { get; set; }
    public decimal SubTotal        { get; set; }
    public string? Nota            { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ProductName { get => Concept; set => Concept = value ?? string.Empty; }
}
