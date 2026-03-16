namespace MaxPlus.IPTV.Core.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Sube un archivo a Cloudinary. 
    /// </summary>
    /// <param name="fileStream">Stream del archivo.</param>
    /// <param name="fileName">Nombre del archivo.</param>
    /// <param name="folder">Carpeta destino.</param>
    /// <param name="publicId">ID opcional para el archivo. Si se proporciona y overwrite es true, reemplaza el existente.</param>
    /// <param name="overwrite">Si debe sobrescribir un archivo con el mismo publicId.</param>
    /// <returns>URL segura del archivo subido.</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string? publicId = null, bool overwrite = false);
}
