using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Nexora.Application.Interfaces;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Product.Command;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Product.Services;

public class ListingService:IListingService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProductBlobStorage _storage;
    private readonly ICategoryService _categoryService;
    private readonly IProductRepository _productRepository;
    

    public ListingService(UserManager<ApplicationUser> userManager,  IProductBlobStorage storage, ICategoryService categoryService, IProductRepository productRepository)
    {
        _userManager = userManager;
        _storage = storage;
        _categoryService = categoryService;
        _productRepository = productRepository;
    }
    
    public async Task<IResult> AddProduct(CreateProductCommand data, string id)
    {
        if (data==null)
        {
            throw new BadHttpRequestException("There is no data for creating ad");
        }

        Domain.Entities.Category category = await _categoryService.FindByName(data.Category);
        if (category == null)
        {
            throw new BadHttpRequestException("Failed to get category");
        }
        Console.WriteLine(category.Id);
        ApplicationUser? user = await _userManager.FindByIdAsync(id);
        Console.WriteLine(user?.Id);
        if (user == null)
        {
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
        Console.WriteLine(product.Name);
        var createdId = await _productRepository.CreateProduct(product);
        if (createdId == Guid.Empty)
        {
            throw new Exception("Failed to create Exception");
        }
        IReadOnlyList<ProductImage> photos = await _storage.UploadAsync(data.Photos, $"listings/{data.Name}", product.Id, product);
        if (photos.Count < 1)
        {
            throw new Exception("Failed to upload photos");
        }
        
        product.Images = photos.ToList();
        return Results.Ok(new {message = "The listing was created"});
    } 
    
    public async Task<IResult> GetProductsService(GetProductsCommand request)
    {
        if (request.Equals(null))
        {
            throw new BadHttpRequestException("There is no any data for getting listings. Please try again");
        }

        var response = await _productRepository.GetAllProduct(request);
        if (response == null)
        {
            throw new BadHttpRequestException("Failed to fetch listings");
        }

        return Results.Ok(new {message = "Listings are fetched", data = response});



    }

    public async Task<IResult> GetProductById(Guid id)
    {
        if (id.Equals(null))
        {
            throw new BadHttpRequestException("There is no id for getting Product");
        }

        
        var listing = await _productRepository.GetProductById(id);
        if (listing == null)
        {
            throw new BadHttpRequestException("There is no listing");
        }
        return Results.Ok(new { message = "Retrieved sucessfully", data =new ProductDto(listing.Name, listing.Description, listing.Price, listing.StockQuantity, listing.isActive, listing.SellerId,listing.Category?.Name, 
            listing.Images.Select(photo=>photo.Url),listing.Reviews.Select(r=>new ReviewDto(r.Author?.FirstName + " "+r.Author?.LastName,
                r.Rating, r.Comment)))  });
    }
    
    public async Task<IResult> UpdateProductHandler(UpdateProductCommand request)
    {
        Listing? product = await _productRepository.GetProductById(request.listingId);
        if (product == null)
        {
            throw new NotFoundException(nameof(Listing), request.listingId);
        }

        product?.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        if (request.PhotosForDelete!=null && request.PhotosForDelete.Count>0)
        {
            var result = await _storage.DeleteForEditing(request.PhotosForDelete);
            if (!result) throw new BlobStorageException("Failed to delete photos from storage");
        }

        if (request.Photos != null && request.Photos.Count>0)
        {
            var result = await _storage.UploadAsync(request.Photos, $"listings/{product?.Name}", product.Id, product);
            if (result.Count < 0) throw new BlobStorageException("Failed to upload new images for listing");
        }

        return Results.Ok(new { message = "The listing was updated", data = product?.Id });
    }

    public async Task<IResult?> RemoveListing(Guid listingId, string userId)
    {
        return await _productRepository.DeleteProduct(listingId, userId);
    }


}