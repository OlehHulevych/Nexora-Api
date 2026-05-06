namespace Nexora.Application.Interfaces.Repositories;

public interface ICartRepository
{
    public Task<bool> CreateCart(Domain.Entities.Cart cart);
    public Task<Domain.Entities.Cart?> GetCartById(Guid id);
    public Task<Domain.Entities.Cart?> GetCartByUserId(string id);
}