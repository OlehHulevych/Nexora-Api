namespace Nexora.Application.Interfaces.Repositories;

public interface IBaseRepository<T, TId>
{
    public Task<bool> Add(T? item);
    public Task<bool> Update(T item);
    public Task<T?> GetById(TId? id);
    public Task<bool> Delete(TId? id);
}