using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class CartRepository:ICartRepository
{
    private readonly IApplicationDbContext _context;

    public CartRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> CreateCart(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async  Task<Cart?> GetCartById(Guid id)
    {
        Cart? cart = await _context.Carts.FirstOrDefaultAsync(c=>c.Id == id);
        if (cart == null) throw new NotFoundException(nameof(Cart), id);
        return cart;
    }

    public async Task<Cart?> GetCartByUserId(string id)
    {
        Cart? cart = await _context.Carts.FirstOrDefaultAsync(c=>c.UserId == id);
        if (cart == null) throw new NotFoundException(nameof(Cart), id);
        return cart;
    }
}