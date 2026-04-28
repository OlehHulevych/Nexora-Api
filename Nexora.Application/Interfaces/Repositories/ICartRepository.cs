using Microsoft.AspNetCore.Http;
using Nexora.Application.Cart.Requests;

namespace Nexora.Application.Interfaces.Repositories;

public interface ICartRepository
{
    public Task<IResult> AddItemToCart(Guid? listingId, string? userId);
    public Task<IResult> RemoveItemFromCart(Guid? id);
    public Task<IResult> ChangingQuantity(ChangingQuantityRequest request);

}