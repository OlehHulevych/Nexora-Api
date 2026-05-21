using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.Repositories;

public interface IReviewLikeRepository
{
    Task<bool> Create(ReviewLike review);
    Task<ReviewLike?> GetById(Guid? id);
    Task<IList<ReviewLike>> GetAllByReviewId(Guid reviewId);
    Task<bool> UpdateAsync(ReviewLike review);
    Task<bool> DeleteAsync(Guid? id);
    Task<ReviewLike?> GetByReviewIdAndUserId(Guid reviewId, string userId);
    Task<bool> Exists(ReviewLike reviewLike);
}