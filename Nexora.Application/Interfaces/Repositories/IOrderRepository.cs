namespace Nexora.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    public Task<bool> CreateOrder(Domain.Entities.Order order);
    public Task<bool> UpdateOrder(Domain.Entities.Order order);
    public Task<Domain.Entities.Order> GetOrderByUser(string id);
    public Task<Domain.Entities.Order> GetOrderById(Guid id);
    public Task<bool> Delete(Guid id);

}