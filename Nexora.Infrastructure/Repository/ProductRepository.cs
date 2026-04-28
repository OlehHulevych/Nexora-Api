using Azure.Core.GeoJson;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Services;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;

namespace Nexora.Infrastructure.Repository;

public class ProductRepository:IProductRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CreateProduct _createProduct;
    private readonly GetProduct _getProduct;
    private readonly IApplicationDbContext _context;
    private readonly IProductBlobStorage _blobStorage;
    private readonly UpdateProductService _updateProductService;

    public ProductRepository(CreateProduct createProduct, GetProduct getProduct, IApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductBlobStorage blobStorage, UpdateProductService updateProductService)
    {
        _updateProductService = updateProductService;
        _createProduct = createProduct;
        _getProduct = getProduct;
        _context = context;
        _userManager = userManager;
        _blobStorage = blobStorage;
    }
    public async Task<IResult> CreateProduct(CreateProductCommand request, string id)
    {
        var response = await _createProduct.AddProduct(request, id);
        if (response.Equals(null))
        {
            return Results.BadRequest(new {message = "Something went wrong"});
        }

        return Results.Ok(new { message = "Listing was created", Listring = response });

    }

    public async Task<IResult> GetAllProduct(GetProductsCommand request)
    {
        var response = await _getProduct.GetProductsService(request);
        if (response.Equals(null))
        {
            return Results.BadRequest(new { message = "Something went wrong" });
        }
        return Results.Ok(new { message="Listings are fetched", listrings = response.Products });
    }

    public async Task<IResult> GetProductById(Guid id)
    {
        var listing = await _getProduct.GetProductById(id);
        if (listing == null)
        {
            return Results.BadRequest("Failed to get listing");
        }

        return Results.Ok(new {message = "Listing is retrieved", listing = listing});



    }

    public async Task<IResult> UpdateProduct(UpdateProductCommand request)
    {
        var id = await _updateProductService.UpdateProductHandler(request);
        if (id == Guid.Empty)
        {
            return Results.BadRequest($"Failed to update listing");
        }
        return  Results.Ok($"Listing {id} was updated");
    }

    public async Task<IResult> DeleteProduct(Guid listingId, string userId)
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
            var result = await _blobStorage.DeleteAsync(images);
        }

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();
        return Results.Ok($"The listing {listingId} was deleted");
    }
}