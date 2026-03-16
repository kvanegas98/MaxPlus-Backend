namespace MaxPlus.IPTV.Core.Entities;

public class Invoice
{
    public Guid     Id              { get; set; }
    public string   NumeroOrden     { get; set; } = string.Empty;
    public string   CustomerName    { get; set; } = string.Empty;
    public Guid?    CustomerId      { get; set; }
    public Guid     UserId          { get; set; }
    public string   OrderType       { get; set; } = "Venta";
    public string?  PaymentMethod   { get; set; }
    public decimal? AmountReceived  { get; set; }
    public decimal  DiscountAmount  { get; set; }
    public decimal  TotalAmount     { get; set; }
    public string   Status          { get; set; } = "Pagada";
    public DateTime SaleDate        { get; set; }
    public string?  Nota            { get; set; }

    public string   PublicLinkToken { get; set; } = Guid.NewGuid().ToString("N");

    public Guid?    SubscriptionId   { get; set; }
    public Guid?    ServiceOrderId   { get; set; }
    public Guid?    MetodoPagoId           { get; set; }
    public string?  PaymentReference       { get; set; }

    // Datos del método de pago (JOIN en SP — solo lectura)
    public string?  MetodoPagoBanco        { get; set; }
    public string?  MetodoPagoTipoCuenta   { get; set; }
    public string?  MetodoPagoNumeroCuenta { get; set; }
    public string?  MetodoPagoTitular      { get; set; }

    public Customer? Customer { get; set; }
    public List<InvoiceDetail> Details { get; set; } = new();
}
