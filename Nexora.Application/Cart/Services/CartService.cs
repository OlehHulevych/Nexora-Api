using Microsoft.AspNetCore.Http;
using Nexora.Application.Cart.Requests;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Cart.Services;

public class CartService:ICartService
{
    private readonly ICartItemRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    public CartService(ICartItemRepository cartRepository, IProductRepository productRepository)
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
        await _cartRepository.AddItemToCart(item);

        return Results.Ok(new {message = "The listing was added to cart", data = item});
    }

    public async Task<IResult> RemoveListing(Guid? id)
    {
        if (id == Guid.Empty || id.Equals(null)) throw new BadHttpRequestException("There is no id for removing");
        await _cartRepository.RemoveItemFromCart(id);

        return Results.Ok(new {message = "The listing was removed from cart"});

    }

    public async Task<IResult> ChangeListingQuantity(ChangingQuantityRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
        }

        await _cartRepository.ChangingQuantity(request);
        return Results.Ok(new {message = "The quantity of items was changed"});




    }
}