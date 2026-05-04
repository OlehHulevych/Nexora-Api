using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Cart.Requests;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.Constants;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Users.Services;

public class CartService:ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<IResult> AddListing(Guid? listingId, string? userId)
    {
        if (listingId == null || userId == null) throw new ArgumentException();
        if (userId == null) throw new ArgumentException();
        Domain.Entities.Cart? userCart = await _cartRepository.GetCartByUser(userId);
        if (userCart == null) throw new NotFoundException(nameof(Domain.Entities.Cart), userId);
        var listing = await _productRepository.GetProductById(listingId);
        if (listing == null) throw new NotFoundException(nameof(Listing), listingId);
        CartItem item = new CartItem()
        {
            CartId = userCart.Id,
            Cart = userCart,
            ListingId = listing.Id,
            Listing = listing,
            Quantity = 1,
            Price = listing.Price
        };


        return Results.Ok(new {message = "The listing was created", data = item});
    }

    public async Task<IResult> RemoveListing(Guid? id)
    {
        if (id == Guid.Empty || id.Equals(null)) throw new BadHttpRequestException("There is no id for removing");
        await _cartRepository.RemoveItemFromCart(id);

        return Results.Ok(new {message = "The listing was removed from cart"});

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