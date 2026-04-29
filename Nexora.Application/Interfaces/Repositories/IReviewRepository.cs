using Microsoft.AspNetCore.Http;
using Nexora.Application.Review.Request;

namespace Nexora.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    public Task<IResult> AddReviewToListing(ReviewRequest request);
    public Task<IResult> AddLikeToReview(ReviewRequest request);
    public Task<IResult> AnswerOnReview(AnswerOnReviewRequest request);
}