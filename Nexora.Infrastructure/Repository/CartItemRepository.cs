using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class CartItemRepository : IBaseRepository<CartItem, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CartItemRepository> _logger;

    public CartItemRepository(IApplicationDbContext context, ILogger<CartItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Add(CartItem? cartItem)
    {
        _logger.LogInformation("Adding cart item for listing {ListingId} to cart {CartId}",
            cartItem?.ListingId, cartItem?.CartId);

        if (cartItem == null)
        {
            _logger.LogWarning("Add cart item failed — cartItem is null");
            throw new BadHttpRequestException("There is no data for adding item to cart");
        }

        try
        {
            await _context.CartItems.AddAsync(cartItem);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Cart item {CartItemId} added successfully", cartItem.Id);
            else
                _logger.LogWarning("Cart item for listing {ListingId} was not saved", cartItem.ListingId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add cart item for listing {ListingId} to cart {CartId}",
                cartItem.ListingId, cartItem.CartId);
            throw;
        }
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation("Deleting cart item {CartItemId}", id);
        try
        {
            await _context.CartItems.Where(i => i.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Cart item {CartItemId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete cart item {CartItemId}", id);
            throw;
        }
    }

    public async Task<bool> Update(CartItem? item)
    {
        _logger.LogInformation("Updating cart item {CartItemId}", item?.Id);

        if (item == null)
        {
            _logger.LogWarning("Update cart item failed — item is null");
            return false;
        }

        try
        {
            _context.CartItems.Update(item);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Cart item {CartItemId} updated successfully", item.Id);
            else
                _logger.LogWarning("Cart item {CartItemId} was not updated", item.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update cart item {CartItemId}", item.Id);
            throw;
        }
    }

    public async Task<CartItem?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching cart item {CartItemId}", id);
        var item = await _context.CartItems.FirstOrDefaultAsync(ct => ct.Id == id);
        if (item == null)
            _logger.LogWarning("Cart item {CartItemId} not found", id);
        return item;
    }
}