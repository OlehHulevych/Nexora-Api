using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Nexora.Infrastructure.Storage;

public class ProductBlobStorageService:IProductBlobStorage
{
    private readonly BlobContainerClient _containerClient;
    private readonly IApplicationDbContext _context;

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    private static string BuildFolderPath(string folder, string filename)
    {
        var normalizedFolder = folder.Trim('/').Replace('\\', '/');
        return $"{normalizedFolder}/{filename}";
    }

    public ProductBlobStorageService(IOptions<BlobStorageOptions> options, IApplicationDbContext context)
    {
        var client = new BlobServiceClient(options.Value.ConnectionString);
        _context = context; 
        _containerClient = client.GetBlobContainerClient(options.Value.ContainerName);
    }
    public async Task<IReadOnlyList<ProductImage>> UploadAsync(List<IFormFile> files, string folder, Guid id, Listing product, CancellationToken ct = default)
    {
        var images = new List<ProductImage>();
        foreach (IFormFile file in files )
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ValidationException("This extension is not allowed for uploading");
            }

            var blobPath = BuildFolderPath(folder, file.FileName);
            var blobClient = _containerClient.GetBlobClient(blobPath);
            await using var filestream = file.OpenReadStream();
            await blobClient.UploadAsync(filestream, new BlobHttpHeaders { ContentType = file.ContentType },
                cancellationToken: ct);
            var uri = blobClient.Uri.ToString();
            ProductImage image = new ProductImage(id, uri, blobPath);
            images.Add(image);
        }

        await _context.ProductImages.AddRangeAsync(images,ct);
        await _context.SaveChangesAsync(ct);
        return images.AsReadOnly();

    }

    public async Task<bool> DeleteAsync(ICollection<ProductImage> photoList)
    {
        if (!photoList.Any())
        {
            return false;
        }

        foreach (ProductImage photo in photoList )
        {
            var blobClient = _containerClient.GetBlobClient(photo.FilePath);
            await blobClient.DeleteIfExistsAsync();
            
        }
        _context.ProductImages.RemoveRange(photoList);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteForEditing(ICollection<string> photoList)
    {
        if (photoList.Count < 0)
        {
            return false;
        }

        foreach (var path in photoList )
        {
            var photoImage = await _context.ProductImages.FirstOrDefaultAsync(i=>i.Url == path);
            if (photoImage is null)
            {
                throw new NotFoundException(nameof(ProductImage), path);
            }

            var blobClient = _containerClient.GetBlobClient(photoImage.FilePath);
            await blobClient.DeleteIfExistsAsync();
             _context.ProductImages.Remove(photoImage);
             await _context.SaveChangesAsync();


        }

        await _context.SaveChangesAsync();

        return true;
    }
}