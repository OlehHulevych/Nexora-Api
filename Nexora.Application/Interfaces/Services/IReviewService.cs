using Microsoft.AspNetCore.Http;
using Nexora.Application.Review.Request;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IReviewService
{
    public Task<IResult> AddReview(ReviewRequest? request);
    public Task<IResult> AnswerOnReview(AnswerOnReviewRequest? request);
    public Task<IResult> RemoveReview(Guid? id);
}