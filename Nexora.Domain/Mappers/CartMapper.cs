using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;

namespace Nexora.Domain.Mappers;

public static class CartMapper
{
    public static CartDto ToDto(Cart cart )
    {
        return new CartDto(cart.Id, cart.User.FirstName + " " +cart.User.LastName, 
            cart.items.Select(item=> new CartItemDto(item.Id,item.Listing.Name,
                item.Price,item.Quantity,item.Quantity*item.Price)).ToList());
    }
}