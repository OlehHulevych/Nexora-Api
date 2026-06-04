using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Storage;

public class UserBlobStorageService : IUserBlobStorage
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

    public async Task<(string, string)> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        _logger.LogInformation("Uploading avatar file {FileName} to folder {Folder}", file.FileName, folder);

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Upload failed — extension {Extension} is not allowed for file {FileName}",
                extension, file.FileName);
            throw new InvalidOperationException($"File type '{extension}' is not allowed");
        }

        try
        {
            var blobPath = BuildBlobPath(folder, file.FileName);
            var blobClient = _containerClient.GetBlobClient(blobPath);

            await using var fileStream = file.OpenReadStream();
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = file.ContentType },
                cancellationToken: ct);

            var uri = blobClient.Uri.ToString();
            _logger.LogInformation("Avatar {FileName} uploaded successfully to {Uri}", file.FileName, uri);
            return (uri, blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload avatar {FileName} to folder {Folder}", file.FileName, folder);
            throw;
        }
    }

    public async Task<(string, string)> UpdateAsync(IFormFile file, ApplicationUser user,
        Avatar? avatarForUpdate, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating avatar for user {UserId}", user.Id);

        if (avatarForUpdate == null)
        {
            _logger.LogWarning("Update avatar failed — avatar is null for user {UserId}", user.Id);
            throw new BadHttpRequestException("Avatar is required");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Update avatar failed — extension {Extension} is not allowed for file {FileName}",
                extension, file.FileName);
            throw new InvalidOperationException($"File type '{extension}' is not allowed");
        }

        try
        {
            _logger.LogInformation("Deleting old avatar blob {BlobPath} for user {UserId}",
                avatarForUpdate.FilePath, user.Id);

            var blobClient = _containerClient.GetBlobClient(avatarForUpdate.FilePath);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);

            _logger.LogInformation("Old avatar deleted for user {UserId} — uploading new one", user.Id);

            var result = await UploadAsync(file, $"avatars/{user.FirstName + "_" + user.LastName}", ct);

            _logger.LogInformation("Avatar updated successfully for user {UserId}", user.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update avatar for user {UserId}", user.Id);
            throw;
        }
    }
}