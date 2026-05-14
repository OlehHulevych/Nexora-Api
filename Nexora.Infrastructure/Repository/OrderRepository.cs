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
    public async Task<bool> CreateOrder(Order order)
    {
        var exist = await _context.Orders.AnyAsync(item =>item.Id == order.Id );
        if (exist) throw new ConflictException("This order already exists.");
        await _context.Orders.AddAsync(order);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateOrder(Order order)
    {
        _context.Orders.Update(order);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Order> GetOrderByUser(string id)
    {
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.BuyerId == id);
        if (order == null) throw new NotFoundException(nameof(Order), id);
        return order;
    }

    public async Task<Order> GetOrderById(Guid id)
    {
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.Id==id);
        if (order == null) throw new NotFoundException(nameof(Order), id);
        return order;
    }

    public async Task<bool> Delete(Guid id)
    {
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.Id==id);
        if (order == null) throw new NotFoundException(nameof(Order), id);
        var result = await _context.SaveChangesAsync();
        return result > 0;

    }
}