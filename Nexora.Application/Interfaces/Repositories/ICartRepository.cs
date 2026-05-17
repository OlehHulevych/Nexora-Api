namespace Nexora.Application.Interfaces.Repositories;

public interface ICartRepository
{
    public Task<bool> Create(Cart cart);
    public Task<Cart?> GetById(Guid id);
    public Task<Cart?> GetByUserId(string id);
    public Task<bool> Update(Cart cart);
}