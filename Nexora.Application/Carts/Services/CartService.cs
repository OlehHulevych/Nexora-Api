using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexora.Application.Carts.Requests;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;
using Nexora.Domain.Mappers;

namespace Nexora.Application.Carts.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IBaseRepository<CartItem, Guid> _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository cartRepository, IBaseRepository<CartItem, Guid> cartItemRepository,
        IProductRepository productRepository, ILogger<CartService> logger)
    {
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<IResult> AddListing(Guid? listingId, string? userId)
    {
        _logger.LogInformation("Adding listing {ListingId} to cart for user {UserId}", listingId, userId);

        if (listingId == null || userId == null)
        {
            _logger.LogWarning("Add to cart failed — listingId or userId is null");
            throw new ArgumentException("ListingId and UserId are required");
        }

        Cart? userCart = await _cartRepository.GetByUserId(userId);
        if (userCart == null)
        {
            _logger.LogWarning("Add to cart failed — cart not found for user {UserId}", userId);
            throw new NotFoundException(nameof(Cart), userId);
        }

        var listing = await _productRepository.GetById(listingId);
        if (listing == null)
        {
            _logger.LogWarning("Add to cart failed — listing {ListingId} not found", listingId);
            throw new NotFoundException(nameof(Listing), listingId);
        }

        CartItem item = new CartItem()
        {
            CartId = userCart.Id,
            ListingId = listing.Id,
            Quantity = 1,
            Price = listing.Price
        };

        await _cartItemRepository.Add(item);
        userCart.items.Add(item);
        await _cartRepository.Update(userCart);

        _logger.LogInformation("Listing {ListingId} added to cart for user {UserId}", listingId, userId);
        CartItemDto dto = CartItemMapper.ToDto(item);
        return Results.Ok(new { message = "The listing was added to cart", data = dto });
    }

    public async Task<IResult> RemoveListing(Guid id)
    {
        _logger.LogInformation("Removing cart item {CartItemId}", id);

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Remove from cart failed — id is empty");
            throw new BadHttpRequestException("There is no id for removing");
        }

        await _cartItemRepository.Delete(id);

        _logger.LogInformation("Cart item {CartItemId} removed successfully", id);
        return Results.Ok(new { message = "The listing was removed from cart" });
    }

    public async Task<IResult> ChangeListingQuantity(ChangingQuantityRequest request)
    {
        _logger.LogInformation("Changing quantity for cart item {CartItemId} — action {Action}",
            request.cartItemId, request.action);

        CartItem? cartItem = await _cartItemRepository.GetById(request.cartItemId);
        if (cartItem == null)
        {
            _logger.LogWarning("Change quantity failed — cart item {CartItemId} not found", request.cartItemId);
            throw new NotFoundException(nameof(CartItem), request.cartItemId);
        }

        var previousQuantity = cartItem.Quantity;

        if (request.action == ActsNames.Increase)
        {
            if (cartItem.Quantity < cartItem.Listing.StockQuantity)
            {
                cartItem.Quantity++;
                _logger.LogInformation("Cart item {CartItemId} quantity increased from {Previous} to {Current}",
                    request.cartItemId, previousQuantity, cartItem.Quantity);
            }
            else
            {
                _logger.LogWarning("Cart item {CartItemId} quantity cannot exceed stock {Stock}",
                    request.cartItemId, cartItem.Listing.StockQuantity);
            }
        }
        else
        {
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                _logger.LogInformation("Cart item {CartItemId} quantity decreased from {Previous} to {Current}",
                    request.cartItemId, previousQuantity, cartItem.Quantity);
            }
            else
            {
                _logger.LogWarning("Cart item {CartItemId} quantity cannot go below 1", request.cartItemId);
            }
        }

        await _cartItemRepository.Update(cartItem);

        _logger.LogInformation("Cart item {CartItemId} quantity updated successfully", request.cartItemId);
        return Results.Ok(new { message = "The quantity of items was changed" });
    }

    public async Task<IResult> GetCart(string id)
    {
        _logger.LogInformation("Fetching cart for user {UserId}", id);

        Cart? cart = await _cartRepository.GetByUserId(id);
        if (cart == null)
        {
            _logger.LogWarning("Get cart failed — cart not found for user {UserId}", id);
            throw new NotFoundException(nameof(Cart), id);
        }

        _logger.LogInformation("Cart fetched successfully for user {UserId} — {ItemCount} items",
            id, cart.items.Count);

        CartDto dto = CartMapper.ToDto(cart);
        return Results.Ok(new { message = "Cart was retrieved", cart = dto });
    }
}