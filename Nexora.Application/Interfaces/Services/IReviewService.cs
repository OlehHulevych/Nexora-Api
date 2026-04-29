using Nexora.Application.Review.Request;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IReviewService
{
    public Task<ReviewDto?> AddReview(ReviewRequest? request);
    public Task<Guid?> LikeReview(Guid? reviewId);
    public Task<ReviewDto?> AnswerOnReview(AnswerOnReviewRequest? request);
}