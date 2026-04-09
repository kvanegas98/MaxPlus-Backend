namespace MaxPlus.IPTV.Core.Entities;

public class ServiceOrderItem
{
    public Guid     Id             { get; set; }
    public Guid     ServiceOrderId { get; set; }
    public Guid     TipoServicioId { get; set; }
    public int      DurationMonths { get; set; } = 1;
    public int      Cantidad       { get; set; } = 1;
    public decimal  UnitPrice      { get; set; }
    public decimal  SubTotal       { get; set; }
    public Guid?    SubscriptionId { get; set; }
    public DateTime CreatedAt      { get; set; }

    // JOIN fields
    public string?  ServiceName    { get; set; }
    public string?  Plataforma     { get; set; }
    public string?  ImageUrl       { get; set; }
}
