using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Product.Command;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;

namespace Nexora.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IProductBlobStorage _blobStorage;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(IApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IProductBlobStorage blobStorage, ILogger<ProductRepository> logger)
    {
        _context = context;
        _userManager = userManager;
        _blobStorage = blobStorage;
        _logger = logger;
    }

    public async Task<Guid?> Create(Listing listing)
    {
        _logger.LogInformation("Creating listing {ListingName} for seller {SellerId}", listing.Name, listing.SellerId);
        try
        {
            await _context.Listings.AddAsync(listing);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Listing {ListingId} created successfully", listing.Id);
            return listing.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create listing {ListingName} for seller {SellerId}", listing.Name, listing.SellerId);
            throw;
        }
    }

    public async Task<IQueryable<Listing>?> GetAll(GetProductsCommand request)
    {
        _logger.LogInformation("Fetching all listings");
        return _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Include(l => l.Reviews)
                .ThenInclude(r => r.Author)
            .AsQueryable();
    }

    public async Task<Listing?> GetById(Guid? id)
    {
        _logger.LogInformation("Fetching listing {ListingId}", id);
        var listing = await _context.Listings.FirstOrDefaultAsync(l => l.Id.Equals(id));
        if (listing == null)
            _logger.LogWarning("Listing {ListingId} not found", id);
        return listing;
    }

    public async Task? Update(Listing listing)
    {
        _logger.LogInformation("Updating listing {ListingId}", listing.Id);
        try
        {
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Listing {ListingId} updated successfully", listing.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update listing {ListingId}", listing.Id);
            throw;
        }
    }

    public async Task<IResult?> Delete(Guid listingId, string userId)
    {
        _logger.LogInformation("Deleting listing {ListingId} by user {UserId}", listingId, userId);

        if (userId.Equals(null))
        {
            _logger.LogWarning("Delete listing failed — userId is null");
            return Results.BadRequest("There is no verifying");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Delete listing failed — user {UserId} not found", userId);
            throw new UnauthorizedAccessException();
        }

        var roles = await _userManager.GetRolesAsync(user);
        bool isAdmin = roles.Any(r => r == nameof(UserRole.ADMIN));
        _logger.LogInformation("User {UserId} isAdmin: {IsAdmin}", userId, isAdmin);

        if (listingId.Equals(null))
        {
            _logger.LogWarning("Delete listing failed — listingId is null");
            return Results.BadRequest("There is no product for deletion");
        }

        var listing = await _context.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId);

        if (listing == null)
        {
            _logger.LogWarning("Delete listing failed — listing {ListingId} not found", listingId);
            return Results.BadRequest("Listing is not found");
        }

        if (listing.SellerId != userId && !isAdmin)
        {
            _logger.LogWarning("Delete listing forbidden — user {UserId} is not the seller or admin", userId);
            return Results.Forbid();
        }

        if (listing.Images.Count > 0)
        {
            _logger.LogInformation("Deleting {Count} images from blob storage for listing {ListingId}",
                listing.Images.Count, listingId);
            await _blobStorage.DeleteAsync(listing.Images);
            _logger.LogInformation("Images deleted from blob storage for listing {ListingId}", listingId);
        }

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Listing {ListingId} deleted successfully by user {UserId}", listingId, userId);
        return Results.Ok($"The listing {listingId} was deleted");
    }
}