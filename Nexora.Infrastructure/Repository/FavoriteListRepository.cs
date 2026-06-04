using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class FavoriteListRepository : IFavoriteListRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FavoriteListRepository> _logger;

    public FavoriteListRepository(IApplicationDbContext context, ILogger<FavoriteListRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Add(FavoriteList? item)
    {
        _logger.LogInformation("Adding favorite list for user {UserId}", item?.UserId);

        if (item == null)
        {
            _logger.LogWarning("Add favorite list failed — item is null");
            throw new ArgumentException("Favorite list cannot be null");
        }

        try
        {
            await _context.FavoriteLists.AddAsync(item);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Favorite list {ListId} added successfully for user {UserId}", item.Id, item.UserId);
            else
                _logger.LogWarning("Favorite list for user {UserId} was not saved", item.UserId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add favorite list for user {UserId}", item.UserId);
            throw;
        }
    }

    public async Task<bool> Update(FavoriteList? item)
    {
        _logger.LogInformation("Updating favorite list {ListId}", item?.Id);

        if (item == null)
        {
            _logger.LogWarning("Update favorite list failed — item is null");
            throw new ArgumentException("Favorite list cannot be null");
        }

        try
        {
            _context.FavoriteLists.Update(item);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Favorite list {ListId} updated successfully", item.Id);
            else
                _logger.LogWarning("Favorite list {ListId} was not updated", item.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update favorite list {ListId}", item.Id);
            throw;
        }
    }

    public async Task<FavoriteList?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching favorite list {ListId}", id);

        var list = await _context.FavoriteLists
            .Include(fl => fl.FavoriteItems)
                .ThenInclude(i => i.Listing)
            .FirstOrDefaultAsync(fl => fl.Id == id);

        if (list == null)
            _logger.LogWarning("Favorite list {ListId} not found", id);
        else
            _logger.LogInformation("Favorite list {ListId} fetched with {Count} items", id, list.FavoriteItems.Count);

        return list;
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation("Deleting favorite list {ListId}", id);
        try
        {
            await _context.FavoriteLists.Where(fl => fl.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Favorite list {ListId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete favorite list {ListId}", id);
            throw;
        }
    }

    public async Task<FavoriteList?> GetByUserId(string id)
    {
        _logger.LogInformation("Fetching favorite list for user {UserId}", id);

        var list = await _context.FavoriteLists
            .Include(fl => fl.FavoriteItems)
                .ThenInclude(i => i.Listing)
            .AsNoTracking()
            .FirstOrDefaultAsync(fl => fl.UserId == id);

        if (list == null)
            _logger.LogWarning("Favorite list not found for user {UserId}", id);
        else
            _logger.LogInformation("Favorite list fetched for user {UserId} with {Count} items", id, list.FavoriteItems.Count);

        return list;
    }
}