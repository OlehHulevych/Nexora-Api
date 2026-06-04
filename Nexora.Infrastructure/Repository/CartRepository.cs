using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class CartRepository : ICartRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CartRepository> _logger;

    public CartRepository(IApplicationDbContext context, ILogger<CartRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Add(Cart? cart)
    {
        _logger.LogInformation("Creating cart for user {UserId}", cart?.UserId);

        if (cart == null)
        {
            _logger.LogWarning("Add cart failed — cart is null");
            throw new ArgumentException("Cart cannot be null");
        }

        try
        {
            await _context.Carts.AddAsync(cart);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Cart {CartId} created successfully for user {UserId}", cart.Id, cart.UserId);
            else
                _logger.LogWarning("Cart for user {UserId} was not saved", cart.UserId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create cart for user {UserId}", cart.UserId);
            throw;
        }
    }

    public async Task<Cart?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching cart {CartId}", id);

        Cart? cart = await _context.Carts
            .Include(c => c.items)
                .ThenInclude(item => item.Listing)
                    .ThenInclude(l => l.Images)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cart == null)
        {
            _logger.LogWarning("Cart {CartId} not found", id);
            throw new NotFoundException(nameof(Cart), id);
        }

        _logger.LogInformation("Cart {CartId} fetched with {ItemCount} items", id, cart.items.Count);
        return cart;
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation("Deleting cart {CartId}", id);
        try
        {
            await _context.Carts.Where(c => c.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Cart {CartId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete cart {CartId}", id);
            throw;
        }
    }

    public async Task<Cart?> GetByUserId(string id)
    {
        _logger.LogInformation("Fetching cart for user {UserId}", id);

        Cart? cart = await _context.Carts
            .Include(c => c.items)
                .ThenInclude(i => i.Listing)
                    .ThenInclude(l => l.Images)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == id);

        if (cart == null)
        {
            _logger.LogWarning("Cart not found for user {UserId}", id);
            throw new NotFoundException(nameof(Cart), id);
        }

        _logger.LogInformation("Cart fetched for user {UserId} with {ItemCount} items", id, cart.items.Count);
        return cart;
    }

    public async Task<bool> Update(Cart cart)
    {
        _logger.LogInformation("Updating cart {CartId}", cart.Id);
        try
        {
            _context.Carts.Update(cart);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Cart {CartId} updated successfully", cart.Id);
            else
                _logger.LogWarning("Cart {CartId} was not updated", cart.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update cart {CartId}", cart.Id);
            throw;
        }
    }
}