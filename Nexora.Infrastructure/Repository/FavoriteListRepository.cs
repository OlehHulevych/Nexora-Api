using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class FavoriteListRepository:IFavoriteListRepository
{
    public Task<bool> Add(FavoriteList item)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Update(FavoriteList item)
    {
        throw new NotImplementedException();
    }

    public Task<FavoriteList> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<FavoriteList> GetByUserId()
    {
        throw new NotImplementedException();
    }
}