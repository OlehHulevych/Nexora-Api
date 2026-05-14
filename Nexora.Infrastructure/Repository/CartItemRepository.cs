using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Carts.Requests;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Constants;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class CartItemRepository:ICartItemRepository
{
    private readonly IApplicationDbContext _context;

    public CartItemRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Guid?> Add(CartItem? cartItem)
    {
        if (cartItem == null) throw new BadHttpRequestException("There is no data for adding item to cart");
        await _context.CartItems.AddAsync(cartItem);
        return cartItem.Id;
    }

    

    public async Task Remove(Guid? id)
    {
        if (id.Equals(null) || id == Guid.Empty) throw new BadHttpRequestException("Id is required");
        CartItem? cartItem = await _context.CartItems.FirstOrDefaultAsync(ct=>ct.Id == id);
        if (cartItem == null) throw new NotFoundException(nameof(CartItem), id);
        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();


    }

    public async Task<bool> Update(CartItem item)
    {
        _context.CartItems.Update(item);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
    

    public async Task<Cart> GetCartByUser(string userId)
    {
        Cart? cart = await _context.Carts.FirstOrDefaultAsync(c=>c.UserId==userId);
        if (cart == null) throw new NotFoundException(nameof(Cart), userId);
        return cart;
    }

    public async Task<CartItem?> GetCartItemById(Guid? id)
    {
        if (id == null) throw new BadHttpRequestException("Id is required");
        return await _context.CartItems.FirstOrDefaultAsync(ct=>ct.Id==id);
    }
}