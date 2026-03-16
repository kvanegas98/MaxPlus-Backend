using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs.PlataformaConfig;

public class PlataformaConfigUpsertDto
{
    [Required]
    [MaxLength(50)]
    public string Plataforma { get; set; } = string.Empty;  // clave interna, ej: "Netflix"

    [Required]
    [MaxLength(100)]
    public string NombreAmigable { get; set; } = string.Empty; // nombre en mensajes, ej: "Netflix"

    [Required]
    [MaxLength(50)]
    public string LabelUsuario { get; set; } = "Usuario";  // "Usuario" | "Correo"

    public bool TieneUrl   { get; set; } = false;
    public bool TienePin   { get; set; } = false;
    public bool TieneCorreo { get; set; } = false;
}
