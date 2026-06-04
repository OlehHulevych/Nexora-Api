using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class ListingPhotoRepository : IListingPhotoRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ListingPhotoRepository> _logger;

    public ListingPhotoRepository(IApplicationDbContext context, ILogger<ListingPhotoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Add(IList<ProductImage> images)
    {
        _logger.LogInformation("Adding {Count} product images", images.Count);
        try
        {
            await _context.ProductImages.AddRangeAsync(images);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("{Count} product images added successfully", images.Count);
            else
                _logger.LogWarning("Product images were not saved");
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add {Count} product images", images.Count);
            throw;
        }
    }

    public async Task<bool> Update(ProductImage image)
    {
        _logger.LogInformation("Updating product image {ImageId}", image.Id);
        try
        {
            _context.ProductImages.Update(image);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Product image {ImageId} updated successfully", image.Id);
            else
                _logger.LogWarning("Product image {ImageId} was not updated", image.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product image {ImageId}", image.Id);
            throw;
        }
    }

    public async Task<ProductImage?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching product image {ImageId}", id);
        ProductImage? productImage = await _context.ProductImages.FirstOrDefaultAsync(i => i.Id == id);
        if (productImage == null)
        {
            _logger.LogWarning("Product image {ImageId} not found", id);
            throw new NotFoundException(nameof(ProductImage), id);
        }
        return productImage;
    }

    public async Task<IList<ProductImage>> GetByListingId(Guid id)
    {
        _logger.LogInformation("Fetching images for listing {ListingId}", id);
        IList<ProductImage> images = await _context.ProductImages
            .Where(i => i.ProductId == id)
            .ToListAsync();
        _logger.LogInformation("Fetched {Count} images for listing {ListingId}", images.Count, id);
        return images;
    }

    public async Task<ProductImage> GetByPath(string path)
    {
        _logger.LogInformation("Fetching product image by path {Path}", path);
        ProductImage? image = await _context.ProductImages.FirstOrDefaultAsync(i => i.FilePath == path);
        if (image == null)
        {
            _logger.LogWarning("Product image not found for path {Path}", path);
            throw new NotFoundException(nameof(ProductImage), path);
        }
        return image;
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation("Deleting product image {ImageId}", id);
        try
        {
            await _context.ProductImages.Where(pi => pi.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Product image {ImageId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product image {ImageId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteRange(IList<ProductImage> images)
    {
        _logger.LogInformation("Deleting {Count} product images", images.Count);
        try
        {
            _context.ProductImages.RemoveRange(images);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("{Count} product images deleted successfully", images.Count);
            else
                _logger.LogWarning("Product images were not deleted");
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete {Count} product images", images.Count);
            throw;
        }
    }
}