using Microsoft.AspNetCore.Http;
using Nexora.Application.Cart.Requests;

namespace Nexora.Application.Interfaces.Repositories;

public interface ICartRepository
{
    public Task<Guid?> AddItemToCart(CartItem cartItem);
    public Task RemoveItemFromCart(Guid? id);
    public Task ChangingQuantity(ChangingQuantityRequest request);
    public Task<Domain.Entities.Cart> GetCartByUser(string userId);
    public Task<CartItem?> GetCartItemById(Guid? id);

}