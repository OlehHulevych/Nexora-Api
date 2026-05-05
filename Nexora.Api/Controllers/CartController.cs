using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Cart.Requests;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Infrastructure.Repository;

namespace Nexora.Api.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController:Controller
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddItemToCart([FromBody] AddingItemToCart request)
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _cartService.AddListing(request.listingId, id);
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<IResult> ChangeItemToCart([FromBody] ChangingQuantityRequest request)
    {
        return await _cartService.ChangeListingQuantity(request);
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IResult> RemoveItemFromCart([FromQuery] Guid id)
    {
        return await _cartService.RemoveListing(id);
    }
}