namespace MaxPlus.IPTV.Core.Entities;

/// <summary>
/// Configuracion: fila única con los ajustes globales del negocio.
/// </summary>
public class Settings
{
    public int      Id                      { get; set; }  // Siempre 1
    public string   BusinessName            { get; set; } = string.Empty;
    public string?  Phone                   { get; set; }
    public string?  Description             { get; set; }
    public string?  Address                 { get; set; }
    public string?  LogoUrl                 { get; set; }
    public decimal  ExchangeRateUSD         { get; set; }
    public string?  DemoPhpBaseUrl          { get; set; }
    public bool     PublicMenuEnabled       { get; set; }
    public bool     DemoAutoApprove         { get; set; }
    public DateTime? UpdatedAt              { get; set; }
}
