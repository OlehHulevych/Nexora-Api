using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class FavoriteItemRepository : IBaseRepository<FavoriteItem, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FavoriteItemRepository> _logger;

    public FavoriteItemRepository(IApplicationDbContext context, ILogger<FavoriteItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Add(FavoriteItem? item)
    {
        _logger.LogInformation("Adding favorite item for listing {ListingId} to list {FavoriteListId}",
            item?.ListingId, item?.FavoriteListId);

        if (item == null)
        {
            _logger.LogWarning("Add favorite item failed — item is null");
            throw new ArgumentException("Favorite item cannot be null");
        }

        try
        {
            await _context.FavoriteItems.AddAsync(item);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Favorite item {ItemId} added successfully", item.Id);
            else
                _logger.LogWarning("Favorite item for listing {ListingId} was not saved", item.ListingId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add favorite item for listing {ListingId}", item.ListingId);
            throw;
        }
    }

    public async Task<bool> Update(FavoriteItem? item)
    {
        _logger.LogInformation("Updating favorite item {ItemId}", item?.Id);

        if (item == null)
        {
            _logger.LogWarning("Update favorite item failed — item is null");
            throw new ArgumentException("Favorite item cannot be null");
        }

        try
        {
            _context.FavoriteItems.Update(item);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Favorite item {ItemId} updated successfully", item.Id);
            else
                _logger.LogWarning("Favorite item {ItemId} was not updated", item.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update favorite item {ItemId}", item.Id);
            throw;
        }
    }

    public async Task<FavoriteItem?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching favorite item {ItemId}", id);
        var item = await _context.FavoriteItems.FirstOrDefaultAsync(i => i.Id == id);
        if (item == null)
            _logger.LogWarning("Favorite item {ItemId} not found", id);
        return item;
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation("Deleting favorite item {ItemId}", id);
        try
        {
            await _context.FavoriteItems.Where(fi => fi.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Favorite item {ItemId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete favorite item {ItemId}", id);
            throw;
        }
    }
}