using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Product.Command;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Product.Services;

public class UpdateProductService
{
    private readonly IApplicationDbContext _context;
    private readonly IProductBlobStorage _blobStorage;

    public UpdateProductService(IApplicationDbContext context, IProductBlobStorage blobStorage)
    {
        _context = context;
        _blobStorage = blobStorage;
    }

    public async Task<Guid> UpdateProductHandler(UpdateProductCommand request)
    {
        var product = await _context.Listings.FirstOrDefaultAsync(l => l.Id == request.listingId);
        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.listingId);
        }

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        if (request.PhotosForDelete!=null && request.PhotosForDelete.Count>0)
        {
            var result = await _blobStorage.DeleteForEditing(request.PhotosForDelete);
            if (!result) throw new BlobStorageException("Failed to delete photos from storage");
        }

        if (request.Photos != null && request.Photos.Count>0)
        {
            var result = await _blobStorage.UploadAsync(request.Photos, $"listings/{product.Name}", product.Id, product);
            if (result.Count < 0) throw new BlobStorageException("Failed to upload new images for listing");
        }

        _context.Listings.Update(product);
        await _context.SaveChangesAsync();
        return product.Id;
    }
}