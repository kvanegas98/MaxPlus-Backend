namespace MaxPlus.IPTV.Application.DTOs.ServiceType;

public class PlataformaConfigDto
{
    public string   Plataforma    { get; set; } = string.Empty;
    public string[] Campos        { get; set; } = [];
    public string   LabelUsuario  { get; set; } = "Usuario";
    public bool     TieneUrl      { get; set; }
    public bool     TienePin      { get; set; }
}
