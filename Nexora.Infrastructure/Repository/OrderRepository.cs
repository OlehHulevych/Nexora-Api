using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(IApplicationDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Add(Order? order)
    {
        _logger.LogInformation("Adding order for buyer {BuyerId}", order?.BuyerId);

        if (order == null)
        {
            _logger.LogWarning("Add order failed — order is null");
            throw new ArgumentException("Order cannot be null");
        }

        try
        {
            await _context.Orders.AddAsync(order);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Order {OrderId} added successfully for buyer {BuyerId}",
                    order.Id, order.BuyerId);
            else
                _logger.LogWarning("Order for buyer {BuyerId} was not saved", order.BuyerId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add order for buyer {BuyerId}", order.BuyerId);
            throw;
        }
    }

    public async Task<bool> Update(Order order)
    {
        _logger.LogInformation("Updating order {OrderId}", order.Id);
        try
        {
            _context.Orders.Update(order);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Order {OrderId} updated successfully", order.Id);
            else
                _logger.LogWarning("Order {OrderId} was not updated", order.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update order {OrderId}", order.Id);
            throw;
        }
    }

    public async Task<List<Order>> GetByUser(string userId)
    {
        _logger.LogInformation("Fetching orders for user {UserId}", userId);
        try
        {
            var orders = await _context.Orders
                .Include(o => o.DeliveredAddress)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.BuyerId == userId)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} orders for user {UserId}", orders.Count, userId);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch orders for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Order?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching order {OrderId}", id);

        var order = await _context.Orders
            .Include(o => o.DeliveredAddress)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            _logger.LogWarning("Order {OrderId} not found", id);
        else
            _logger.LogInformation("Order {OrderId} fetched with {ItemCount} items", id, order.Items.Count);

        return order;
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation("Deleting order {OrderId}", id);
        try
        {
            await _context.Orders.Where(o => o.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Order {OrderId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete order {OrderId}", id);
            throw;
        }
    }
}