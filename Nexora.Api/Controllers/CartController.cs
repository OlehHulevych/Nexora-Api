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
    /// <summary>
    /// Adding listing to cart
    /// </summary>
    /// <param name="request">Request contain listing id and cart id</param>
    /// <returns>Added favorite item</returns>
    /// <response code="200">Item added successfully</response>
    /// <response code="404">Failed to add item to cart</response>
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddItemToCart([FromBody] AddingItemToCart request)
    {
        _logger.LogInformation("Cart controller - Adding item to cart is started");
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _cartService.AddListing(request.listingId, id);
    }
    
    /// <summary>
    /// Get cart list of listings 
    /// </summary>
    /// <returns>list of listing</returns>
    /// <response code="200">List was fetched successfully</response>
    /// <response code="404">Failed to fetch items or cart does not  exist</response>
    [Authorize]
    [HttpGet]
    public async Task<IResult> GetCart()
    {
        _logger.LogInformation("Cart controller - Getting cart by user");
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _cartService.GetCart(id);
    }
    
    /// <summary>
    /// Changing quantity of item in cart
    /// </summary>
    /// <returns>items with changed quantity of item</returns>
    /// <response code="200">Quantity of item was changed</response>
    /// <response code="404">Listing or favorite list not found</response>
    [Authorize]
    [HttpPatch]
    public async Task<IResult> ChangeItemToCart([FromBody] ChangingQuantityRequest request)
    {
        _logger.LogInformation("Cart controller - changing quantity of item in cart");
        return await _cartService.ChangeListingQuantity(request);
    }
    
    /// <summary>
    /// Removing item from cart
    /// </summary>
    /// <response code="200">Item was deleted</response>
    /// <response code="404">Items does not exist</response>
    [Authorize]
    [HttpDelete]
    public async Task<IResult> RemoveItemFromCart([FromQuery] Guid id)
    {
        _logger.LogInformation("Cart controller - removing item from cart");
        return await _cartService.RemoveListing(id);
    }
}