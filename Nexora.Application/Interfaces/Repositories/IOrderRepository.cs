namespace Nexora.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    public Task<bool> CreateOrder(Order order);
    public Task<bool> UpdateOrder(Order order);
    public Task<Order> GetOrderByUser(string id);
    public Task<Order> GetOrderById(Guid id);
    public Task<bool> Delete(Guid id);

}