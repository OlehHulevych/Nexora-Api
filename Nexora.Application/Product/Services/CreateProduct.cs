using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Nexora.Application.Category.Services;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Product.Command;



namespace Nexora.Application.Product.Services;

public class CreateProduct:ICreateProduct
{
    private readonly IApplicationDbContext _context;
    private readonly IProductBlobStorage _storage;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CategoryService _categoryService;

    public CreateProduct(IApplicationDbContext context, IProductBlobStorage storage, CategoryService categoryService, UserManager<ApplicationUser> userManager)
    {
        _storage = storage;
        _categoryService = categoryService;
        _context = context;
        _userManager = userManager;
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
}