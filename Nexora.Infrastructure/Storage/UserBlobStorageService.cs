using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Storage;

public class UserBlobStorageService:IUserBlobStorage
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<UserBlobStorageService> _logger;
    
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    private static string BuildBlobPath(string folder, string filename)
    {
        var normalizedFolder = folder.Trim('/').Replace('\\', '/');
        return $"{normalizedFolder}/{filename}";
    }


    public UserBlobStorageService(IOptions<BlobStorageOptions> options, ILogger<UserBlobStorageService> logger)
    {
        var client = new BlobServiceClient(options.Value.ConnectionString);
        _containerClient = client.GetBlobContainerClient(options.Value.ContainerName);
        _logger = logger;
    }
    
    public async Task<(string,string)> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type '{extension}' is not allowed ");
        }

        var blobPath = BuildBlobPath( folder, file.FileName);
        var blobClient = _containerClient.GetBlobClient(blobPath);
        await using var fileStream = file.OpenReadStream();
        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = file.ContentType},
            cancellationToken: ct);
        return (blobClient.Uri.ToString(),blobPath);
    }

    public async Task<(string,string)> UpdateAsync(IFormFile file, ApplicationUser user, Avatar avatarForUpdate, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type '{extension}' is not allowed");
        }

        var blobName = avatarForUpdate.FilePath;
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken:ct);
        return await UploadAsync(file,$"avatars/{user.FirstName + "_" +user.LastName}");
    }
}