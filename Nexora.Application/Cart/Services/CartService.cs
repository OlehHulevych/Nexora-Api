using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Cart.Requests;
using Nexora.Application.Interfaces.Context;
using Nexora.Domain.Constants;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Users.Services;

public class CartService
{
    private readonly IApplicationDbContext _context;

    public CartService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartItem?> AddListing(Guid? listingId, string? userId)
    {
        if (listingId == null || userId == null) throw new ArgumentException();
        if (userId == null) throw new ArgumentException();

        var userCart = await _context.Carts.FirstOrDefaultAsync(c=>c.UserId==userId);
        if (userCart == null) throw new NotFoundException(nameof(Domain.Entities.Cart), userId);

        var listing = await _context.Listings.FirstOrDefaultAsync(l=>l.Id == listingId);
        if (listing == null)
        {
            throw new NotFoundException(nameof(Listing), listingId);
        }

        CartItem item = new CartItem()
        {
            CartId = userCart.Id,
            Cart = userCart,
            ListingId = listing.Id,
            Listing = listing,
            Quantity = 1,
            Price = listing.Price
        };

        await _context.CartItems.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;

    }

    public async Task<Guid?> RemoveListing(Guid? id)
    {
        if (id == Guid.Empty || id.Equals(null)) throw new BadHttpRequestException("There is no id for removing");
        CartItem? cartItem = await _context.CartItems.FirstOrDefaultAsync(ct => ct.Id.Equals(id));
        if (cartItem is null) throw new BadHttpRequestException("listing in cart is not found");
        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        return cartItem.Id;

    }

    public async Task<Guid?> ChangeListingQuantity(ChangingQuantityRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
        }

        CartItem? cartItem = await _context.CartItems.Include(ct=>ct.Listing).FirstOrDefaultAsync(c=>c.Id == request.cartItemId);
        if (cartItem is null) throw new NotFoundException(nameof(CartItem), request.cartItemId);
        if (request.action == ActsNames.Increase)
        {
            if (cartItem.Quantity != cartItem.Listing.StockQuantity)
            {
                cartItem.Quantity += 1;
            }
            
        }

        if (request.action == ActsNames.Reduce)
        {
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity -= 1;
            }
        }

        await _context.SaveChangesAsync();

        return cartItem.Id;


    }
}