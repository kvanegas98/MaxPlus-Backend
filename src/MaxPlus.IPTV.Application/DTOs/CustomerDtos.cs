using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

public class CustomerCreateDto
{
    [Required(ErrorMessage = "El nombre del cliente es requerido.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    public string  Name     { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
    public string? Phone    { get; set; }

    [MaxLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
    public string? Address  { get; set; }

    [MaxLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres.")]
    public string? Email    { get; set; }
}

public class CustomerUpdateDto
{
    [Required(ErrorMessage = "El nombre del cliente es requerido.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    public string  Name     { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
    public string? Phone    { get; set; }

    [MaxLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
    public string? Address  { get; set; }

    [MaxLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres.")]
    public string? Email    { get; set; }
}

public class CustomerResponseDto
{
    public Guid     Id                  { get; set; }
    public string   Name                { get; set; } = string.Empty;
    public string?  Phone               { get; set; }
    public string?  Address             { get; set; }
    public string?  Email               { get; set; }
    public bool     IsActive            { get; set; }
    public DateTime CreatedAt           { get; set; }
}
