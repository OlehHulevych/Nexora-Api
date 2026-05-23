using Microsoft.AspNetCore.Http;

namespace Nexora.Application.Interfaces.Services;

public interface IFavoriteService
{
    public Task<IResult> AddItemToList(string userId, Guid listingId);
    public Task<IResult> GetFavoriteList(string userId);
    public Task<IResult> DeleteItemFromList(Guid id);
}