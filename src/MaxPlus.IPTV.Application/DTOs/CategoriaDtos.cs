using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

public class CategoriaCreateDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(100)]
    public string  Nombre      { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    [MaxLength(7)]
    public string  Color       { get; set; } = "#8B5CF6";

    public int     Orden       { get; set; } = 0;
}

public class CategoriaUpdateDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(100)]
    public string  Nombre      { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    [MaxLength(7)]
    public string  Color       { get; set; } = "#8B5CF6";

    public int     Orden       { get; set; } = 0;
    public bool    IsActive    { get; set; } = true;
}

public class CategoriaResponseDto
{
    public Guid     Id          { get; set; }
    public string   Nombre      { get; set; } = string.Empty;
    public string?  Descripcion { get; set; }
    public string   Color       { get; set; } = "#8B5CF6";
    public int      Orden       { get; set; }
    public bool     IsActive    { get; set; }
    public DateTime CreatedAt   { get; set; }
}
