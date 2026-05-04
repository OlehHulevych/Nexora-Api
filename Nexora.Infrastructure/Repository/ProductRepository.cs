using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;

namespace Nexora.Infrastructure.Repository;

public class ProductRepository:IProductRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IProductBlobStorage _blobStorage;

    public ProductRepository(IApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductBlobStorage blobStorage)
    {
        _context = context;
        _userManager = userManager;
        _blobStorage = blobStorage;
    }
    public async Task<Guid?> CreateProduct(Listing listing)
    {
        await _context.Listings.AddAsync(listing);
        await _context.SaveChangesAsync();
        return listing.Id;

    }

    

    public async Task<GetProductResponse?> GetAllProduct(GetProductsCommand request)
    {
        IQueryable<Listing> queries = _context.Listings.Include(l => l.Category).Include(l => l.Images)
            .Include(l => l.Reviews).ThenInclude(r => r.Author).Include(l => l.Category).AsQueryable();
        int length = await _context.Listings.CountAsync();
        List<ProductDto> listings = await queries.OrderBy(l => l.CreatedAt).Skip((request.page - 1) * 10).Take(10)
            .Select(l => new ProductDto(l.Name, l.Description, l.Price, l.StockQuantity, l.isActive, l.SellerId,
                l.Category!.Name, l.Images.Select(i => i.Url), l.Reviews
                    .Select(r => new ReviewDto(r.Author!.FirstName + " " + r.Author.LastName, r.Rating, r.Comment))))
            .ToListAsync();
        if (listings.Count < 1)
        {
            throw new BadHttpRequestException("Failed to fetch listings");
        }

        return new GetProductResponse(listings, request.page, length / 10);
    }

    public async Task<Listing?> GetProductById(Guid? id)
    {
        return await _context.Listings.FirstOrDefaultAsync(l => l.Id.Equals(id));
    }

    public async Task? UpdateProduct(Listing listing)
    {
        _context.Listings.Update(listing);
        await _context.SaveChangesAsync();
    }

    public async Task<IResult?> DeleteProduct(Guid listingId, string userId)
    {
        if (userId.Equals(null))
        {
            return Results.BadRequest("There is no verifying");
        }
        Console.WriteLine(userId);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }
        Console.WriteLine(user.FirstName);
        var roles = await _userManager.GetRolesAsync(user);

        bool isAdmin = false;
        foreach (var role in roles)
        {
            if (role == nameof(UserRole.ADMIN))
            {
                isAdmin = true;
            }
        }
        
        
        
        if (listingId.Equals(null))
        {
            return Results.BadRequest("There is no product for deletion");
        }

        var listing = await _context.Listings.Include(l=>l.Images).FirstOrDefaultAsync(listing=>listing.Id == listingId);
        if (listing == null)
        {
            return Results.BadRequest("Listing is not found");
        }

        if (listing.SellerId != userId || !isAdmin)
        {
            return Results.Forbid();
        }
        var images = listing.Images;
        if (images.Count>0)
        {
            await _blobStorage.DeleteAsync(images);
        }

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();
        return Results.Ok($"The listing {listingId} was deleted");
    }
}