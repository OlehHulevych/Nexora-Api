using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class CartItemRepository:IBaseRepository<CartItem, Guid>
{
    private readonly IApplicationDbContext _context;

    public CartItemRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Add(CartItem? cartItem)
    {
        if (cartItem == null) throw new BadHttpRequestException("There is no data for adding item to cart");
        await _context.CartItems.AddAsync(cartItem);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    

    public async Task<bool> Delete(Guid id)
    {
        await _context.CartItems.Where(i=>i.Id==id).ExecuteDeleteAsync();
        return await _context.SaveChangesAsync() > 0;


    }

    public async Task<bool> Update(CartItem? item)
    {
        if (item != null) _context.CartItems.Update(item);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
    

  

    public async Task<CartItem?> GetById(Guid id)
    {
        return await _context.CartItems.FirstOrDefaultAsync(ct=>ct.Id==id);
    }

   
}