using Microsoft.AspNetCore.Http;
using Nexora.Application.Carts.Requests;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Interfaces.Services;

public interface ICartService
{
    public Task<IResult> AddListing(Guid? listingId, string? userId);

    public Task<IResult> RemoveListing(Guid? id);

    public  Task<IResult> ChangeListingQuantity(ChangingQuantityRequest request);
}