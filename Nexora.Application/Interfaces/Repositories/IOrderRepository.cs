namespace Nexora.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    public Task<Guid> CreateOrder();
    public Task<Guid> ChangeOrderStatus();
    public Task<Guid> UpdateOrder();
}