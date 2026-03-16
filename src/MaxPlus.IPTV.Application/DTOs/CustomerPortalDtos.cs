namespace MaxPlus.IPTV.Application.DTOs;

public class CustomerPortalResponseDto
{
    public Guid    CustomerId    { get; set; }
    public string  CustomerName  { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    public List<CustomerPortalSubscriptionDto> Subscriptions { get; set; } = [];
    public List<CustomerPortalDemoDto>         Demos         { get; set; } = [];
    public List<CustomerPortalInvoiceDto>      Invoices      { get; set; } = [];
}

public class CustomerPortalSubscriptionDto
{
    public Guid     Id             { get; set; }
    public string?  ServiceName    { get; set; }
    public string   Status         { get; set; } = string.Empty;
    public DateTime StartDate      { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int      DaysRemaining  { get; set; }

    // Credenciales — solo se retornan si la suscripción está Active
    public string?  AccessUser     { get; set; }
    public string?  AccessPassword { get; set; }
    public string?  PinCode        { get; set; }
    public string?  PlatformUrl    { get; set; }
    public string?  ProfileUser    { get; set; }
    public string?  ProfilePin     { get; set; }
}

public class CustomerPortalDemoDto
{
    public Guid      Id          { get; set; }
    public string?   ServiceName { get; set; }
    public string    Status      { get; set; } = string.Empty;
    public DateTime? ExpiresAt   { get; set; }
    public DateTime  CreatedAt   { get; set; }
}

public class CustomerPortalInvoiceDto
{
    public Guid     Id            { get; set; }
    public string   NumeroOrden   { get; set; } = string.Empty;
    public decimal  TotalAmount   { get; set; }
    public string?  PaymentMethod { get; set; }
    public string?  PaymentReference { get; set; }
    public string   Status        { get; set; } = string.Empty;
    public DateTime SaleDate      { get; set; }
    public List<CustomerPortalInvoiceDetailDto> Details { get; set; } = [];
}

public class CustomerPortalInvoiceDetailDto
{
    public string  Concept  { get; set; } = string.Empty;
    public int     Quantity { get; set; }
    public decimal SubTotal { get; set; }
}
