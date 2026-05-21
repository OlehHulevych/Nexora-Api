using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class OrderRepository:IOrderRepository
{
    private readonly IApplicationDbContext _context;

    public OrderRepository(IApplicationDbContext context)
    {
        _context= context;
    }
    public async Task<bool> Add(Order? order)
    {
        if (order == null) throw new ArgumentException();
        await _context.Orders.AddAsync(order);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> Update(Order order)
    {
        _context.Orders.Update(order);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
    

    public async Task<List<Order>> GetByUser(string userId)
    {
        return await _context.Orders
            .Include(o => o.DeliveredAddress)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.BuyerId == userId)
            .ToListAsync();
    }

    public async Task<Order?> GetById(Guid id)
    {
       return await _context.Orders.Include(o=>o.DeliveredAddress).Include(o=>o.Items).ThenInclude(i=>i.Product).FirstOrDefaultAsync(o => o.Id == id);
       
    }

    public async Task<bool> Delete(Guid id)
    {
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.Id==id);
        if (order == null) throw new NotFoundException(nameof(Order), id);
        var result = await _context.SaveChangesAsync();
        return result > 0;

    }
}