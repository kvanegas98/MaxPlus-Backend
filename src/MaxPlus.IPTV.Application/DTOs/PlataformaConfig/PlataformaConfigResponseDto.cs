namespace MaxPlus.IPTV.Application.DTOs.PlataformaConfig;

public class PlataformaConfigResponseDto
{
    public int      Id             { get; set; }
    public string   Plataforma     { get; set; } = string.Empty;
    public string   NombreAmigable { get; set; } = string.Empty;
    public string   LabelUsuario   { get; set; } = string.Empty;
    public bool     TieneUrl       { get; set; }
    public bool     TienePin       { get; set; }
    public bool     TieneCorreo    { get; set; }
    public string[] Campos         { get; set; } = [];
}
