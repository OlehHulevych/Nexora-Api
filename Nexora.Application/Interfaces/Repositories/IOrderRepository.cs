namespace Nexora.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    public Task<bool> Create(Order order);
    public Task<bool> Update(Order order);
    public Task<List<Order>> GetByUser(string id);
    public Task<Order?> GetById(Guid id);
    public Task<bool> Delete(Guid id);

}