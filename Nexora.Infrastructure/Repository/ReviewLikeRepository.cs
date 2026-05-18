using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class ReviewLikeRepository:IReviewLikeRepository
{
    private readonly IApplicationDbContext _context;

    public ReviewLikeRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Create(ReviewLike reviewlike)
    {
        await _context.ReviewLikes.AddAsync(reviewlike);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<ReviewLike?> GetById(Guid? id)
    {
        return await _context.ReviewLikes.FirstOrDefaultAsync(l=>l.Id==id);
    }

    public async Task<IList<ReviewLike>> GetAllByReviewId(Guid reviewId)
    {
        return await _context.ReviewLikes.Where(l => l.ReviewId == reviewId).ToListAsync();
    }

    public async Task<bool> UpdateAsync(ReviewLike reviewLike)
    {
         _context.ReviewLikes.Update(reviewLike);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid? id)
    {
        ReviewLike? like = await GetById(id);
        if (like == null) throw new NotFoundException(nameof(ReviewLike), id);
        _context.ReviewLikes.Remove(like);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    
}