using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class ReviewRepository : IReviewRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReviewRepository> _logger;

    public ReviewRepository(IApplicationDbContext context, ILogger<ReviewRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(Review review)
    {
        _logger.LogInformation("Creating review for listing {ListingId} by user {AuthorId}",
            review.ProductId, review.AuthorId);
        try
        {
            await _context.Reviews.AddAsync(review);
            var saved = await _context.SaveChangesAsync();
            if (saved > 0)
                _logger.LogInformation("Review {ReviewId} created successfully", review.Id);
            else
                _logger.LogWarning("Review for listing {ListingId} was not saved", review.ProductId);
            return saved > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create review for listing {ListingId} by user {AuthorId}",
                review.ProductId, review.AuthorId);
            throw;
        }
    }

    public async Task<Review?> GetByIdAsync(Guid? id)
    {
        _logger.LogInformation("Fetching review {ReviewId}", id);

        if (id == null)
        {
            _logger.LogWarning("Get review failed — id is null");
            throw new BadHttpRequestException("Id is required");
        }

        Review? review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id.Equals(id));
        if (review == null)
        {
            _logger.LogWarning("Review {ReviewId} not found", id);
            throw new NotFoundException(nameof(Review), id);
        }

        return review;
    }

    public async Task<IList<Review>> GetAllByListingIdAsync(Guid listingId)
    {
        _logger.LogInformation("Fetching reviews for listing {ListingId}", listingId);

        IList<Review> reviews = await _context.Reviews
            .Where(r => r.ProductId == listingId)
            .ToListAsync();

        if (reviews.Count <= 0)
        {
            _logger.LogWarning("No reviews found for listing {ListingId}", listingId);
            throw new NotFoundException(nameof(Review), listingId);
        }

        _logger.LogInformation("Fetched {Count} reviews for listing {ListingId}", reviews.Count, listingId);
        return reviews;
    }

    public async Task<bool> UpdateAsync(Review review)
    {
        _logger.LogInformation("Updating review {ReviewId}", review.Id);
        try
        {
            _context.Reviews.Update(review);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Review {ReviewId} updated successfully", review.Id);
            else
                _logger.LogWarning("Review {ReviewId} was not updated", review.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update review {ReviewId}", review.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid? id)
    {
        _logger.LogInformation("Deleting review {ReviewId}", id);
        try
        {
            await _context.Reviews.Where(i => i.Id == id).ExecuteDeleteAsync();
            _logger.LogInformation("Review {ReviewId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete review {ReviewId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        _logger.LogInformation("Checking if review {ReviewId} exists", id);
        var exists = await _context.Reviews.AnyAsync(r => r.Id == id);
        _logger.LogInformation("Review {ReviewId} exists: {Exists}", id, exists);
        return exists;
    }
}