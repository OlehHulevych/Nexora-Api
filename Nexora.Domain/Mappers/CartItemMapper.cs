using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;

namespace Nexora.Domain.Mappers;

public static class CartItemMapper
{
    public static CartItemDto ToDto(CartItem item)
    {
        return new CartItemDto(item.Id, item.Listing.Name, item.Listing.Price, item.Quantity,
            item.Listing.Price * item.Quantity);
    }
}