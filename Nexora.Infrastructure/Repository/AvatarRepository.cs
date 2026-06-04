using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class AvatarRepository : IAvatarRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AvatarRepository> _logger;

    public AvatarRepository(IApplicationDbContext context, ILogger<AvatarRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Add(Avatar avatar)
    {
        _logger.LogInformation("Adding avatar for user {UserId}", avatar.UserId);
        try
        {
            await _context.Avatars.AddAsync(avatar);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Avatar {AvatarId} added successfully for user {UserId}", avatar.Id, avatar.UserId);
            else
                _logger.LogWarning("Avatar for user {UserId} was not saved", avatar.UserId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add avatar for user {UserId}", avatar.UserId);
            throw;
        }
    }

    public async Task<bool> Update(Avatar avatar)
    {
        _logger.LogInformation("Updating avatar {AvatarId} for user {UserId}", avatar.Id, avatar.UserId);
        try
        {
            _context.Avatars.Update(avatar);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Avatar {AvatarId} updated successfully", avatar.Id);
            else
                _logger.LogWarning("Avatar {AvatarId} was not updated", avatar.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update avatar {AvatarId}", avatar.Id);
            throw;
        }
    }

    public async Task<Avatar?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching avatar {AvatarId}", id);
        Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(a => a.Id == id);
        if (avatar == null)
        {
            _logger.LogWarning("Avatar {AvatarId} not found", id);
            throw new NotFoundException(nameof(Avatar), id);
        }
        return avatar;
    }

    public async Task<Avatar?> GetByUserId(string id)
    {
        _logger.LogInformation("Fetching avatar for user {UserId}", id);
        Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(a => a.UserId == id);
        if (avatar == null)
        {
            _logger.LogWarning("Avatar not found for user {UserId}", id);
            throw new NotFoundException(nameof(Avatar), id);
        }
        return avatar;
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation("Deleting avatar {AvatarId}", id);
        try
        {
            Avatar? avatar = await GetById(id);
            _context.Avatars.Remove(avatar);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Avatar {AvatarId} deleted successfully", id);
            else
                _logger.LogWarning("Avatar {AvatarId} was not deleted", id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete avatar {AvatarId}", id);
            throw;
        }
    }
}