using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Product.Services;

public class ListingService : IListingService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductBlobStorage _storage;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ListingService> _logger;

    public ListingService(IUserRepository userRepository, ICategoryRepository categoryRepository,
        IProductBlobStorage storage, IProductRepository productRepository, ILogger<ListingService> logger)
    {
        _storage = storage;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IResult> AddProduct(CreateProductCommand data, string id)
    {
        _logger.LogInformation("Creating new listing by user {UserId}", id);

        Category? category = await _categoryRepository.GetCategory(data.Category);
        if (category == null)
        {
            _logger.LogWarning("Add product failed — category {Category} not found", data.Category);
            throw new BadHttpRequestException("Failed to get category");
        }
        _logger.LogInformation("Category {CategoryId} found for listing", category.Id);

        ApplicationUser? user = await _userRepository.GetUser(id);
        if (user == null)
        {
            _logger.LogWarning("Add product failed — user {UserId} not found", id);
            throw new BadHttpRequestException("Failed to get user");
        }

        Listing product = new Listing
        {
            Name = data.Name,
            Category = category,
            Price = data.Price,
            StockQuantity = data.StockQuantity,
            CategoryId = category.Id,
            Description = data.Description,
            SellerId = user.Id,
            Seller = user
        };

        var createdId = await _productRepository.Create(product);
        if (createdId == Guid.Empty)
        {
            _logger.LogError("Failed to create listing in database for user {UserId}", id);
            throw new Exception("Failed to create listing");
        }
        _logger.LogInformation("Listing {ListingId} created in database for user {UserId}", createdId, id);

        _logger.LogInformation("Uploading {PhotoCount} photos for listing {ListingId}", data.Photos.Count, createdId);
        IReadOnlyList<ProductImage> photos = await _storage.UploadAsync(data.Photos, $"listings/{data.Name}", product.Id, product);
        if (photos.Count < 1)
        {
            _logger.LogError("Photo upload failed for listing {ListingId}", createdId);
            throw new Exception("Failed to upload photos");
        }
        _logger.LogInformation("{PhotoCount} photos uploaded for listing {ListingId}", photos.Count, createdId);

        product.Images = photos.ToList();
        _logger.LogInformation("Listing {ListingId} created successfully by user {UserId}", createdId, id);
        return Results.Ok(new { message = "The listing was created" });
    }

    public async Task<IResult> GetProductsService(GetProductsCommand request)
    {
        _logger.LogInformation("Fetching listings — page {Page}", request.page);

        if (request.Equals(null))
        {
            _logger.LogWarning("Get products failed — request is null");
            throw new BadHttpRequestException("There is no any data for getting listings. Please try again");
        }

        IQueryable<Listing>? queries = await _productRepository.GetAll(request);
        if (queries == null)
        {
            _logger.LogWarning("Get products failed — repository returned null query");
            throw new BadHttpRequestException("Failed to fetch listings");
        }

        int length = await queries.CountAsync();
        _logger.LogInformation("Found {Total} listings — fetching page {Page}", length, request.page);

        List<ProductDto> listings = await queries
            .OrderBy(l => l.CreatedAt)
            .Skip((request.page - 1) * 10)
            .Take(10)
            .Select(l => new ProductDto(l.Name, l.Description, l.Price, l.StockQuantity, l.isActive, l.SellerId,
                l.Category!.Name, l.Images.Select(i => i.Url),
                l.Reviews.Select(r => new ReviewDto(r.Author!.FirstName + " " + r.Author.LastName, r.Rating, r.Comment))))
            .ToListAsync();

        if (listings.Count < 1)
        {
            _logger.LogWarning("No listings found for page {Page}", request.page);
            throw new BadHttpRequestException("Failed to fetch listings");
        }

        _logger.LogInformation("Fetched {Count} listings out of {Total} — page {Page}", listings.Count, length, request.page);
        return Results.Ok(new { message = "Listings are fetched", data = new GetProductResponse(listings, request.page, length / 10) });
    }

    public async Task<IResult> GetProductById(Guid id)
    {
        _logger.LogInformation("Fetching listing {ListingId}", id);

        if (id.Equals(null))
        {
            _logger.LogWarning("Get product failed — id is null");
            throw new BadHttpRequestException("There is no id for getting Product");
        }

        var listing = await _productRepository.GetById(id);
        if (listing == null)
        {
            _logger.LogWarning("Get product failed — listing {ListingId} not found", id);
            throw new BadHttpRequestException("There is no listing");
        }

        _logger.LogInformation("Listing {ListingId} fetched successfully", id);
        return Results.Ok(new
        {
            message = "Retrieved successfully",
            data = new ProductDto(listing.Name, listing.Description, listing.Price, listing.StockQuantity,
                listing.isActive, listing.SellerId, listing.Category?.Name,
                listing.Images.Select(photo => photo.Url),
                listing.Reviews.Select(r => new ReviewDto(r.Author?.FirstName + " " + r.Author?.LastName, r.Rating, r.Comment)))
        });
    }

    public async Task<IResult> UpdateProductHandler(UpdateProductCommand request)
    {
        _logger.LogInformation("Updating listing {ListingId}", request.listingId);

        Listing? product = await _productRepository.GetById(request.listingId);
        if (product == null)
        {
            _logger.LogWarning("Update listing failed — listing {ListingId} not found", request.listingId);
            throw new NotFoundException(nameof(Listing), request.listingId);
        }

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        await _productRepository.Update(product)!;
        _logger.LogInformation("Listing {ListingId} updated in database", request.listingId);

        if (request.PhotosForDelete != null && request.PhotosForDelete.Count > 0)
        {
            _logger.LogInformation("Deleting {Count} photos for listing {ListingId}", request.PhotosForDelete.Count, request.listingId);
            var result = await _storage.DeleteForEditing(request.PhotosForDelete);
            if (!result)
            {
                _logger.LogError("Failed to delete photos from storage for listing {ListingId}", request.listingId);
                throw new BlobStorageException("Failed to delete photos from storage");
            }
            _logger.LogInformation("Photos deleted from storage for listing {ListingId}", request.listingId);
        }

        if (request.Photos != null && request.Photos.Count > 0)
        {
            _logger.LogInformation("Uploading {Count} new photos for listing {ListingId}", request.Photos.Count, request.listingId);
            var result = await _storage.UploadAsync(request.Photos, $"listings/{product.Name}", product.Id, product);
            if (result.Count < 0)
            {
                _logger.LogError("Failed to upload new photos for listing {ListingId}", request.listingId);
                throw new BlobStorageException("Failed to upload new images for listing");
            }
            _logger.LogInformation("New photos uploaded for listing {ListingId}", request.listingId);
        }

        _logger.LogInformation("Listing {ListingId} updated successfully", request.listingId);
        return Results.Ok(new { message = "The listing was updated", data = product.Id });
    }

    public async Task<IResult?> RemoveListing(Guid listingId, string userId)
    {
        _logger.LogInformation("Removing listing {ListingId} by user {UserId}", listingId, userId);
        var result = await _productRepository.Delete(listingId, userId);
        _logger.LogInformation("Listing {ListingId} removed by user {UserId}", listingId, userId);
        return result;
    }
}