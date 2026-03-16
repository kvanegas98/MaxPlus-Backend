namespace MaxPlus.IPTV.Application.DTOs.ServiceType;

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
    public bool IsActive { get; set; }
}
