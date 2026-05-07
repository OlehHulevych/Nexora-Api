using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class ListingPhotoRepository:IListingPhotoRepository
{
    private readonly IApplicationDbContext _context;
    public ListingPhotoRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Add(IList<ProductImage> images)
    {
        await _context.ProductImages.AddRangeAsync(images);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> Update(ProductImage image)
    {
        _context.ProductImages.Update(image);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<ProductImage?> GetById(Guid id)
    {
        ProductImage? productImage = await _context.ProductImages.FirstOrDefaultAsync(i=>i.Id==id);
        if (productImage == null) throw new NotFoundException(nameof(Avatar), id);
        return productImage;
    }

    public async Task<IList<ProductImage>> GetByListingId(Guid id)
    {
        IList<ProductImage> images = await _context.ProductImages.Where(i => i.ProductId == id).ToListAsync();
        return images;
    }

    public async Task<ProductImage> GetByPath(string path)
    {
        ProductImage? image = await _context.ProductImages.FirstOrDefaultAsync(i=>i.FilePath==path);
        if (image == null) throw new NotFoundException(nameof(ProductImage), path);
        return image;
    }

    public async Task<bool> Delete(Guid id)
    {
        ProductImage? image = await GetById(id);
        if (image == null) throw new NotFoundException(nameof(Avatar), id);
        _context.ProductImages.Remove(image);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteRange(IList<ProductImage> images)
    {
        _context.ProductImages.RemoveRange(images);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}