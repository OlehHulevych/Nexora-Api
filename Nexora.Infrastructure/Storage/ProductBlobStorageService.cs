using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Nexora.Infrastructure.Storage;

public class ProductBlobStorageService : IProductBlobStorage
{
    private readonly BlobContainerClient _containerClient;
    private readonly IListingPhotoRepository _listingPhotoRepository;
    private readonly ILogger<ProductBlobStorageService> _logger;

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    private static string BuildFolderPath(string folder, string filename)
    {
        var normalizedFolder = folder.Trim('/').Replace('\\', '/');
        return $"{normalizedFolder}/{filename}";
    }

    public ProductBlobStorageService(IOptions<BlobStorageOptions> options,
        IListingPhotoRepository listingPhotoRepository, ILogger<ProductBlobStorageService> logger)
    {
        var client = new BlobServiceClient(options.Value.ConnectionString);
        _containerClient = client.GetBlobContainerClient(options.Value.ContainerName);
        _listingPhotoRepository = listingPhotoRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductImage>> UploadAsync(List<IFormFile> files, string folder, Guid id,
        Listing product, CancellationToken ct = default)
    {
        _logger.LogInformation("Uploading {Count} images for listing {ListingId} to folder {Folder}",
            files.Count, id, folder);

        var images = new List<ProductImage>();

        foreach (IFormFile file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Upload failed — extension {Extension} is not allowed for file {FileName}",
                    extension, file.FileName);
                throw new ValidationException("This extension is not allowed for uploading");
            }

            var blobPath = BuildFolderPath(folder, file.FileName);
            _logger.LogInformation("Uploading file {FileName} to blob path {BlobPath}", file.FileName, blobPath);

            try
            {
                var blobClient = _containerClient.GetBlobClient(blobPath);
                await using var filestream = file.OpenReadStream();
                await blobClient.UploadAsync(filestream, new BlobHttpHeaders { ContentType = file.ContentType },
                    cancellationToken: ct);

                var uri = blobClient.Uri.ToString();
                ProductImage image = new ProductImage(id, uri, blobPath);
                images.Add(image);
                _logger.LogInformation("File {FileName} uploaded successfully to {Uri}", file.FileName, uri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FileName} to blob storage", file.FileName);
                throw;
            }
        }

        await _listingPhotoRepository.Add(images);
        _logger.LogInformation("{Count} images uploaded and saved for listing {ListingId}", images.Count, id);
        return images.AsReadOnly();
    }

    public async Task<bool> DeleteAsync(ICollection<ProductImage> photoList)
    {
        _logger.LogInformation("Deleting {Count} images from blob storage", photoList.Count);

        if (!photoList.Any())
        {
            _logger.LogWarning("Delete images failed — photo list is empty");
            return false;
        }

        foreach (ProductImage photo in photoList)
        {
            try
            {
                _logger.LogInformation("Deleting blob {FilePath}", photo.FilePath);
                var blobClient = _containerClient.GetBlobClient(photo.FilePath);
                await blobClient.DeleteIfExistsAsync();
                _logger.LogInformation("Blob {FilePath} deleted successfully", photo.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blob {FilePath}", photo.FilePath);
                throw;
            }
        }

        await _listingPhotoRepository.DeleteRange(photoList.ToList());
        _logger.LogInformation("{Count} images deleted from blob storage and database", photoList.Count);
        return true;
    }

    public async Task<bool> DeleteForEditing(ICollection<string> photoList)
    {
        _logger.LogInformation("Deleting {Count} images for editing", photoList.Count);

        if (photoList.Count <= 0)
        {
            _logger.LogWarning("Delete for editing failed — photo list is empty");
            return false;
        }

        foreach (var path in photoList)
        {
            _logger.LogInformation("Deleting image by path {Path}", path);
            try
            {
                var photoImage = await _listingPhotoRepository.GetByPath(path);

                var blobClient = _containerClient.GetBlobClient(photoImage.FilePath);
                await blobClient.DeleteIfExistsAsync();
                _logger.LogInformation("Blob {FilePath} deleted successfully", photoImage.FilePath);

                await _listingPhotoRepository.Delete(photoImage.Id);
                _logger.LogInformation("Image {ImageId} deleted from database", photoImage.Id);
            }
            catch (Exception ex) when (ex is not NotFoundException)
            {
                _logger.LogError(ex, "Failed to delete image for path {Path}", path);
                throw;
            }
        }

        _logger.LogInformation("{Count} images deleted successfully for editing", photoList.Count);
        return true;
    }
}