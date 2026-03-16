using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

public class SettingsResponseDto
{
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

public class SettingsUpdateDto
{
    [Required(ErrorMessage = "El nombre del negocio es requerido.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres.")]
    public string   BusinessName            { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "El teléfono no puede exceder los 50 caracteres.")]
    public string?  Phone                   { get; set; }

    [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    public string?  Description             { get; set; }

    [MaxLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
    public string?  Address                 { get; set; }

    [MaxLength(500)]
    public string?  LogoUrl                 { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "La tasa de cambio debe ser mayor a 0.")]
    public decimal  ExchangeRateUSD         { get; set; }

    [MaxLength(500)]
    public string?  DemoPhpBaseUrl          { get; set; }

    public bool     PublicMenuEnabled       { get; set; }
    public bool     DemoAutoApprove         { get; set; }
}

public class PublicSettingsDto
{
    public string   BusinessName      { get; set; } = string.Empty;
    public string?  Phone             { get; set; }
    public string?  Description       { get; set; }
    public string?  Address           { get; set; }
    public string?  LogoUrl           { get; set; }
    public decimal  ExchangeRateUSD   { get; set; }
    public bool     PublicMenuEnabled { get; set; }
    public bool     DemoAutoApprove   { get; set; }
}
