using Nexora.Application.Carts.Requests;

namespace Nexora.Application.Interfaces.Repositories;

public interface ICartItemRepository
{
    public Task<Guid?> Add(CartItem cartItem);
    public Task Remove(Guid? id);
    public Task<bool> Update(CartItem item);
    public Task<CartItem?> GetById(Guid? id);

}