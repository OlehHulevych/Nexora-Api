using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Product.Services;

public class ListingService:IListingService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IProductBlobStorage _storage;
    private readonly ICategoryService _categoryService;
    

    public ListingService(UserManager<ApplicationUser> userManager, IApplicationDbContext context, IProductBlobStorage storage, ICategoryService categoryService)
    {
        _userManager = userManager;
        _context = context;
        _storage = storage;
        _categoryService = categoryService;
    }
    
    public async Task<Listing> AddProduct(CreateProductCommand data, string id)
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
        Console.WriteLine(user.Id);
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
        await _context.Listings.AddAsync(product);
        await _context.SaveChangesAsync();
        IReadOnlyList<ProductImage> photos = await _storage.UploadAsync(data.Photos, $"listings/{data.Name}", product.Id, product);
        if (photos.Count < 1)
        {
            throw new Exception("Failed to upload photos");
        }

        product.Images = photos.ToList();
        return product;
        

    } 
    
    public async Task<GetProductResponse> GetProductsService(GetProductsCommand request)
    {
        if (request.Equals(null))
        {
            throw new BadHttpRequestException("There is no any data for getting listings. Please try again");
        }

        IQueryable<Listing> queries =  _context.Listings.Include(l => l.Category).Include(l=>l.Images).Include(l=>l.Reviews).ThenInclude(r=>r.Author).Include(l=>l.Category).AsQueryable();
        int length = await _context.Listings.CountAsync();
        List<ProductDto> listings =  await queries.OrderBy(l=>l.CreatedAt).Skip((request.page-1)*10).Take(10)
            .Select(l=> new ProductDto(l.Name,l.Description,l.Price,l.StockQuantity,l.isActive, l.SellerId,l.Category.Name,l.Images.Select(i=>i.Url),l.Reviews
                .Select(r=>new ReviewDto(r.Author.FirstName + " "+r.Author.LastName,r.Rating, r.Comment )))).ToListAsync();
        if (listings.Count < 1)
        {
            throw new BadHttpRequestException("Failed to fetch listings");
        }

        return new GetProductResponse(listings,request.page,length/10);

    }

    public async Task<ProductDto> GetProductById(Guid id)
    {
        if (id.Equals(null))
        {
            throw new BadHttpRequestException("There is no id for getting Product");
        }

        var listing = await _context.Listings.Include(l => l.Category).Include(l=>l.Reviews)
            .Include(l => l.Images).FirstOrDefaultAsync(l => l.Id.Equals(id));
        if (listing == null)
        {
            throw new BadHttpRequestException("There is no listing");
        }
        return new ProductDto(listing.Name, listing.Description, listing.Price, listing.StockQuantity, listing.isActive, listing.SellerId,listing.Category.Name, listing.Images.Select(photo=>photo.Url),listing.Reviews.Select(r=>new ReviewDto(r.Author.FirstName + " "+r.Author.LastName,r.Rating, r.Comment)));
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
            var result = await _storage.DeleteForEditing(request.PhotosForDelete);
            if (!result) throw new BlobStorageException("Failed to delete photos from storage");
        }

        if (request.Photos != null && request.Photos.Count>0)
        {
            var result = await _storage.UploadAsync(request.Photos, $"listings/{product.Name}", product.Id, product);
            if (result.Count < 0) throw new BlobStorageException("Failed to upload new images for listing");
        }

        _context.Listings.Update(product);
        await _context.SaveChangesAsync();
        return product.Id;
    }


}