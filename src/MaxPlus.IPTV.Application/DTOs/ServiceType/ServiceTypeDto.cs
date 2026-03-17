namespace MaxPlus.IPTV.Application.DTOs.ServiceType;

public class CatalogoAgrupadoDto
{
    public Guid?   CategoriaId     { get; set; }
    public string  CategoriaNombre { get; set; } = "Sin categoría";
    public string  CategoriaColor  { get; set; } = "#8B5CF6";
    public int     Orden           { get; set; }
    public List<ServiceTypeDto> Servicios { get; set; } = [];
}

public class ServiceTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal PurchasePrice { get; set; }
    public int DurationDays { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Plataforma { get; set; } = "IPTV";
    public string? ImageUrl { get; set; }
    public bool    IsActive        { get; set; }
    public Guid?   CategoriaId     { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? CategoriaColor  { get; set; }
}
