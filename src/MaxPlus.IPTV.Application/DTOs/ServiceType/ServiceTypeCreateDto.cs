using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs.ServiceType;

public class ServiceTypeCreateDto
{
    [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo o cero")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "El costo de compra debe ser un valor positivo o cero")]
    public decimal PurchasePrice { get; set; }

    [Required]
    [Range(1, 3650, ErrorMessage = "La duración debe ser entre 1 y 3650 días")]
    public int DurationDays { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "Paid"; // Paid | Demo

    [Required]
    [MaxLength(50)]
    public string Plataforma { get; set; } = "IPTV"; // IPTV | FlujoTV | Netflix | Otro

    [MaxLength(500)]
    public string? ImageUrl    { get; set; }

    public Guid?   CategoriaId { get; set; }
}
