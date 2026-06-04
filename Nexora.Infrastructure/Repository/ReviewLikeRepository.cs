using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class ReviewLikeRepository : IReviewLikeRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReviewLikeRepository> _logger;

    public ReviewLikeRepository(IApplicationDbContext context, ILogger<ReviewLikeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Create(ReviewLike reviewlike)
    {
        _logger.LogInformation("Creating review like for review {ReviewId} by user {AuthorId}",
            reviewlike.ReviewId, reviewlike.AuthorId);
        try
        {
            bool exist = await _context.ReviewLikes.AnyAsync(l =>
                l.ReviewId == reviewlike.ReviewId && l.AuthorId == reviewlike.AuthorId);

            if (exist)
            {
                _logger.LogWarning("Review like already exists for review {ReviewId} by user {AuthorId}",
                    reviewlike.ReviewId, reviewlike.AuthorId);
                throw new ConflictException(nameof(ReviewLike));
            }

            await _context.ReviewLikes.AddAsync(reviewlike);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Review like {LikeId} created successfully for review {ReviewId}",
                    reviewlike.Id, reviewlike.ReviewId);
            else
                _logger.LogWarning("Review like for review {ReviewId} was not saved", reviewlike.ReviewId);
            return result > 0;
        }
        catch (Exception ex) when (ex is not ConflictException)
        {
            _logger.LogError(ex, "Failed to create review like for review {ReviewId} by user {AuthorId}",
                reviewlike.ReviewId, reviewlike.AuthorId);
            throw;
        }
    }

    public async Task<ReviewLike?> GetById(Guid? id)
    {
        _logger.LogInformation("Fetching review like {LikeId}", id);
        var like = await _context.ReviewLikes.FirstOrDefaultAsync(l => l.Id == id);
        if (like == null)
            _logger.LogWarning("Review like {LikeId} not found", id);
        return like;
    }

    public async Task<IList<ReviewLike>> GetAllByReviewId(Guid reviewId)
    {
        _logger.LogInformation("Fetching all likes for review {ReviewId}", reviewId);
        var likes = await _context.ReviewLikes
            .Where(l => l.ReviewId == reviewId)
            .ToListAsync();
        _logger.LogInformation("Fetched {Count} likes for review {ReviewId}", likes.Count, reviewId);
        return likes;
    }

    public async Task<bool> UpdateAsync(ReviewLike reviewLike)
    {
        _logger.LogInformation("Updating review like {LikeId}", reviewLike.Id);
        try
        {
            _context.ReviewLikes.Update(reviewLike);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Review like {LikeId} updated successfully", reviewLike.Id);
            else
                _logger.LogWarning("Review like {LikeId} was not updated", reviewLike.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update review like {LikeId}", reviewLike.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid? id)
    {
        _logger.LogInformation("Deleting review like {LikeId}", id);
        try
        {
            await _context.ReviewLikes.Where(i => i.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Review like {LikeId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete review like {LikeId}", id);
            throw;
        }
    }

    public async Task<ReviewLike?> GetByReviewIdAndUserId(Guid reviewId, string userId)
    {
        _logger.LogInformation("Fetching review like for review {ReviewId} by user {UserId}", reviewId, userId);
        var like = await _context.ReviewLikes
            .Where(l => l.ReviewId == reviewId && l.AuthorId == userId)
            .FirstOrDefaultAsync();
        if (like == null)
            _logger.LogWarning("Review like not found for review {ReviewId} by user {UserId}", reviewId, userId);
        return like;
    }

    public async Task<bool> Exists(ReviewLike reviewLike)
    {
        _logger.LogInformation("Checking if review like exists for review {ReviewId} by user {AuthorId}",
            reviewLike.ReviewId, reviewLike.AuthorId);
        var exists = await _context.ReviewLikes.AnyAsync(l =>
            l.ReviewId == reviewLike.ReviewId && l.AuthorId == reviewLike.AuthorId);
        _logger.LogInformation("Review like exists for review {ReviewId} by user {AuthorId}: {Exists}",
            reviewLike.ReviewId, reviewLike.AuthorId, exists);
        return exists;
    }
}