using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Carts.Requests;
using Nexora.Application.Interfaces.Services;

namespace Nexora.Api.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController:Controller
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;
    public CartController(ICartService cartService, Logger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddItemToCart([FromBody] AddingItemToCart request)
    {
        _logger.LogInformation("Adding item to cart is started");
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _cartService.AddListing(request.listingId, id);
    }

    [Authorize]
    [HttpGet]
    public async Task<IResult> GetCart()
    {
        _logger.LogInformation("Getting cart by user");
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _cartService.GetCart(id);
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<IResult> ChangeItemToCart([FromBody] ChangingQuantityRequest request)
    {
        _logger.LogInformation("Changing quantity of item in cart");
        return await _cartService.ChangeListingQuantity(request);
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IResult> RemoveItemFromCart([FromQuery] Guid id)
    {
        _logger.LogInformation("Removing item from cart");
        return await _cartService.RemoveListing(id);
    }
}