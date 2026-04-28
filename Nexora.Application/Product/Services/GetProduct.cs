using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Product.Services;

public class GetProduct
{
    private readonly IApplicationDbContext _context;
    
    public GetProduct(IApplicationDbContext context)
    {
        _context = context;
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
    
}