using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Review.Request;
using Nexora.Application.Review.Services;

namespace Nexora.Infrastructure.Repository;

public class ReviewRepository:IReviewRepository
{
    private readonly ReviewService _reviewService;

   
    
    public Task<IResult> AddReviewToListing(ReviewRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> AddLikeToReview(ReviewRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> AnswerOnReview(AnswerOnReviewRequest request)
    {
        throw new NotImplementedException();
    }
}