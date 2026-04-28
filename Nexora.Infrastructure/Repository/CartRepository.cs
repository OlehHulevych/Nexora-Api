using Microsoft.AspNetCore.Http;
using Nexora.Application.Cart.Requests;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Services;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class CartRepository:ICartRepository
{
    private readonly CartService _cartService;

    public CartRepository(CartService cartService)
    {
        _cartService = cartService;
    }
    public async Task<IResult> AddItemToCart(Guid? listingId, string? userId)
    {
        if (listingId == null || userId == null) return Results.BadRequest("There is no data for adding item to cart");
        CartItem? cartItem = await _cartService.AddListing(listingId,userId);
        if (cartItem is null) return Results.BadRequest("Failed to add listing to the cart");
        return Results.Ok(new {message="The listing was added successfully"});
    }

    public async Task<IResult> RemoveItemFromCart(Guid? id)
    {
        if (id.Equals(null) || id == Guid.Empty) return Results.BadRequest("No id");
        var deletedId = await _cartService.RemoveListing(id);
        if (deletedId.Equals(null) || id == Guid.Empty) return Results.BadRequest("Failed to delete listing from you cart");
        return Results.Ok(new {message= $"The listing with {deletedId} was deleted", id = deletedId});

    }

    public async Task<IResult> ChangingQuantity(ChangingQuantityRequest request)
    {
        Guid? id = await _cartService.ChangeListingQuantity(request);
        if (id == null) return Results.BadRequest("Failed to change Quantity");
        return Results.Ok(new { message = "Quantity was changed", cartItemId = id });

    }
}