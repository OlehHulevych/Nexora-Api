
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;
namespace Nexora.Infrastructure.Repository;

public class ReviewRepository:IReviewRepository
{
    private readonly IApplicationDbContext _context;
    public ReviewRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> CreateAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        var saved = await _context.SaveChangesAsync();
        return saved > 0;
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        Review? review = await _context.Reviews.FirstOrDefaultAsync(r=>r.Id.Equals(id));
        if (review == null) throw new NotFoundException(nameof(Review), id);
        return review;

    }

    public async Task<IList<Review>> GetAllByListingIdAsync(Guid listingId)
    {
        IList<Review> reviews = await _context.Reviews.Where(r => r.ProductId == listingId).ToListAsync();
        if (reviews.Count <= 0) throw new NotFoundException(nameof(Review), listingId);
        return reviews;
    }

    public async Task<bool> UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        Review? review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review==null) throw new NotFoundException(nameof(Review), id);
        _context.Reviews.Remove(review);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Reviews.AnyAsync(r=>r.Id==id);
    }
}