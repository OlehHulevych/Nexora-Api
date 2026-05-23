namespace Nexora.Application.Interfaces.Repositories;

public interface IFavoriteListRepository:IBaseRepository<FavoriteList, Guid>
{
    public Task<FavoriteList?> GetByUserId(string id);
}