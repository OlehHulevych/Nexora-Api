using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class CartRepository : ICartRepository
{
    private readonly IApplicationDbContext _context;

    public CartRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Add(Cart? cart)
    {
        if (cart == null) throw new ArgumentException();
        await _context.Carts.AddAsync(cart);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Cart?> GetById(Guid id)
    {
        Cart? cart = await _context.Carts.Include(c => c.items)
            .ThenInclude(item => item.Listing).ThenInclude(l => l.Images).Include(c=>c.User).FirstOrDefaultAsync(c => c.Id == id);
        if (cart == null) throw new NotFoundException(nameof(Cart), id);
        return cart;
    }

    public async Task<bool> Delete(Guid id)
    {
        Cart? cart = await GetById(id);
        if (cart == null) throw new NotFoundException(nameof(Cart), id);
        _context.Carts.Remove(cart);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Cart?> GetByUserId(string id)
    {
        Cart? cart = await _context.Carts.Include(c => c.items).ThenInclude(i=>i.Listing).ThenInclude(l => l.Images).Include(c=>c.User).FirstOrDefaultAsync(c => c.UserId == id);
        if (cart == null) throw new NotFoundException(nameof(Cart), id);
        return cart;
    }

    public async Task<bool> Update(Cart cart)
    {
        _context.Carts.Update(cart);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

}