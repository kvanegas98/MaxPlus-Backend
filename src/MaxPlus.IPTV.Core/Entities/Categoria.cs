namespace MaxPlus.IPTV.Core.Entities;

public class Categoria
{
    public Guid     Id          { get; set; }
    public string   Nombre      { get; set; } = string.Empty;
    public string?  Descripcion { get; set; }
    public string   Color       { get; set; } = "#8B5CF6";
    public int      Orden       { get; set; } = 0;
    public bool     IsActive    { get; set; } = true;
    public DateTime CreatedAt   { get; set; }
}
