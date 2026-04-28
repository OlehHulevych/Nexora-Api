namespace Nexora.Application.Interfaces.Repositories;

public interface OrderRepository
{
    public Task<Guid> CreateOrder();
    public Task<Guid> ChangeOrderStatus();
    public Task<Guid> UpdateOrder();
}