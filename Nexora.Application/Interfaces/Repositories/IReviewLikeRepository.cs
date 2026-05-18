namespace Nexora.Application.Interfaces.Repositories;

public interface IReviewLikeRepository
{
    Task<bool> Create(ReviewLike review);
    Task<ReviewLike?> GetById(Guid? id);
    Task<IList<ReviewLike>> GetAllByReviewId(Guid reviewId);
    Task<bool> UpdateAsync(ReviewLike review);
    Task<bool> DeleteAsync(Guid? id);
   
}