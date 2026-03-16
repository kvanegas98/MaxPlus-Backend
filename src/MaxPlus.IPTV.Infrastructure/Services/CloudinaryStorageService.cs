using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MaxPlus.IPTV.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MaxPlus.IPTV.Infrastructure.Services;

public class CloudinaryStorageService : IStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryStorageService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string? publicId = null, bool overwrite = false)
    {
        // Determinamos si es imagen o archivo raw (PDF, etc) basándonos en la extensión
        bool isImage = IsImageExtension(fileName);

        RawUploadParams uploadParams;

        if (isImage)
        {
            uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                PublicId = publicId,
                Overwrite = overwrite,
                Invalidate = overwrite, // Refresca el CDN si estamos sobrescribiendo
                UniqueFilename = string.IsNullOrEmpty(publicId)
            };
        }
        else
        {
            uploadParams = new RawUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                PublicId = publicId,
                Overwrite = overwrite,
                Invalidate = overwrite, 
                Type = "upload", // Asegura acceso público
                UniqueFilename = string.IsNullOrEmpty(publicId)
            };
        }

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    private bool IsImageExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".webp";
    }
}
