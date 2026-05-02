using Nexora.Application.Cart.Requests;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Interfaces.Services;

public interface ICartService
{
    public Task<CartItem?> AddListing(Guid? listingId, string? userId);

    public Task<Guid?> RemoveListing(Guid? id);

    public  Task<Guid?> ChangeListingQuantity(ChangingQuantityRequest request);
}