using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

public class PaymentMethodCreateDto
{
    [Required]
    [MaxLength(100)]
    public string  Nombre       { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Banco        { get; set; }

    [MaxLength(50)]
    public string? TipoCuenta   { get; set; }  // Corriente | Ahorro | Móvil

    [MaxLength(50)]
    public string? NumeroCuenta { get; set; }

    [MaxLength(150)]
    public string? Titular      { get; set; }
}

public class PaymentMethodUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string  Nombre       { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Banco        { get; set; }

    [MaxLength(50)]
    public string? TipoCuenta   { get; set; }

    [MaxLength(50)]
    public string? NumeroCuenta { get; set; }

    [MaxLength(150)]
    public string? Titular      { get; set; }

    public bool IsActive        { get; set; } = true;
}

public class PaymentMethodResponseDto
{
    public Guid    Id            { get; set; }
    public string  Nombre        { get; set; } = string.Empty;
    public string? Banco         { get; set; }
    public string? TipoCuenta    { get; set; }
    public string? NumeroCuenta  { get; set; }
    public string? Titular       { get; set; }
    public bool    IsActive      { get; set; }
    public DateTime CreatedAt    { get; set; }
}
