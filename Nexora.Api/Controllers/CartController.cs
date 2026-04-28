using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Cart.Requests;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Infrastructure.Repository;

namespace Nexora.Api.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController:Controller
{
    private readonly ICartRepository _cartRepository;

    public CartController(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddItemToCart([FromBody] AddingItemToCart request)
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _cartRepository.AddItemToCart(request.listingId, id);
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<IResult> ChangeItemToCart([FromBody] ChangingQuantityRequest request)
    {
        return await _cartRepository.ChangingQuantity(request);
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IResult> RemoveItemFromCart([FromQuery] Guid id)
    {
        return await _cartRepository.RemoveItemFromCart(id);
    }
}