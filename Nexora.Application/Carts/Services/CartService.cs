using Microsoft.AspNetCore.Http;
using Nexora.Application.Carts.Requests;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;
using Nexora.Domain.Mappers;

namespace Nexora.Application.Carts.Services;

public class CartService:ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductRepository productRepository)
    {
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
    }

    public async Task<IResult> AddListing(Guid? listingId, string? userId)
    {
        if (listingId == null || userId == null) throw new ArgumentException();
        if (userId == null) throw new ArgumentException();
        Cart? userCart = await _cartRepository.GetCartByUserId(userId);
        if (userCart == null) throw new NotFoundException(nameof(Cart), userId);
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
        await _cartItemRepository.Add(item);
        userCart.items.Add(item);
        await _cartRepository.UpdateCart(userCart);
        CartItemDto dto = CartItemMapper.ToDto(item);
        return Results.Ok(new {message = "The listing was added to cart", data = dto});
    }

    public async Task<IResult> RemoveListing(Guid? id)
    {
        if (id == Guid.Empty || id.Equals(null)) throw new BadHttpRequestException("There is no id for removing");
        await _cartItemRepository.Remove(id);

        return Results.Ok(new {message = "The listing was removed from cart"});

    }

   

    public async Task<IResult> ChangeListingQuantity(ChangingQuantityRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
        }

        CartItem? cartItem = await _cartItemRepository.GetCartItemById(request.cartItemId);
        if (cartItem == null) throw new NotFoundException(nameof(CartItem), request.cartItemId);
        if (request.action == ActsNames.Increase)
        {
            if (cartItem.Quantity < cartItem.Listing.StockQuantity)
            {
                cartItem.Quantity++;
            }
        }
        else
        {
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
        }

        await _cartItemRepository.Update(cartItem);
        return Results.Ok(new {message = "The quantity of items was changed"});
        
    }

    public Task<IResult> GetCart(string id)
    {
        throw new NotImplementedException();
    }
}