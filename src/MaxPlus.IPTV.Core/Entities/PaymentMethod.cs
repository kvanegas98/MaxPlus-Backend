namespace MaxPlus.IPTV.Core.Entities;

public class PaymentMethod
{
    public Guid    Id            { get; set; }
    public string  Nombre        { get; set; } = string.Empty;
    public string? Banco         { get; set; }
    public string? TipoCuenta    { get; set; }
    public string? NumeroCuenta  { get; set; }
    public string? Titular       { get; set; }
    public bool    IsActive      { get; set; } = true;
    public DateTime CreatedAt    { get; set; }
}
