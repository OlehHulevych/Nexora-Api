namespace Nexora.Application.Interfaces.Repositories;

public interface ICartRepository
{
    public Task<bool> CreateCart(Cart cart);
    public Task<Cart?> GetCartById(Guid id);
    public Task<Cart?> GetCartByUserId(string id);
    public Task<bool> UpdateCart(Cart cart);
}