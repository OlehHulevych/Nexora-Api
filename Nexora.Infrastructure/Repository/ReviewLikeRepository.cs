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
        bool exist = await _context.ReviewLikes.AnyAsync(l =>
            l.ReviewId == reviewlike.ReviewId && l.AuthorId == reviewlike.AuthorId);
        if (exist) throw new ConflictException(nameof(ReviewLike));
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
        await _context.ReviewLikes.Where(i=>i.Id==id).ExecuteDeleteAsync();
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<ReviewLike?> GetByReviewIdAndUserId(Guid reviewId, string userId)
    {
        return await _context.ReviewLikes.Where(l=>l.ReviewId==reviewId && l.AuthorId==userId).FirstOrDefaultAsync();
    }

    public async Task<bool> Exists(ReviewLike reviewLike)
    {
        return await _context.ReviewLikes.AnyAsync(l =>
            l.ReviewId == reviewLike.ReviewId && l.AuthorId == reviewLike.AuthorId);
    }
}