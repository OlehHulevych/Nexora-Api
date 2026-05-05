using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Review.Request;
using Nexora.Application.Review.Services;
using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class ReviewRepository:IReviewRepository
{
    

    public ReviewRepository(IReviewService reviewService)
    {
    }
   
    
    public async Task<IResult> AddReviewToListing(Review review)
    {
        
        
    }

    public async Task<IResult> AddLikeToReview(Guid reviewId)
    {
        var id = await _reviewService.LikeReview(reviewId);
        if (id == null) throw new BadHttpRequestException("Failed to like review");
        return Results.Ok(new {message = "Like was added", data = id});
    }

    public async Task<IResult> AnswerOnReview(AnswerOnReviewRequest request)
    {
        var response = await _reviewService.AnswerOnReview(request);
        if(response == null)  throw new BadHttpRequestException("Failed to answer on review. Something went wrong");
        return Results.Ok(new { message = "answers is sent", data = response });
    }
}