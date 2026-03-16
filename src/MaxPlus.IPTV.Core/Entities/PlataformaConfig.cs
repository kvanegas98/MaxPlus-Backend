namespace MaxPlus.IPTV.Core.Entities;

public class PlataformaConfig
{
    public int    Id             { get; set; }
    public string Plataforma     { get; set; } = string.Empty;
    public string NombreAmigable { get; set; } = string.Empty;
    public string LabelUsuario   { get; set; } = "Usuario";
    public bool   TieneUrl       { get; set; }
    public bool   TienePin       { get; set; }
    public bool   TieneCorreo    { get; set; }
    public bool   IsActive       { get; set; } = true;
}
