using Microsoft.AspNetCore.Http;
using Nexora.Application.Reviews.Request;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IReviewService
{
    public Task<IResult> AddReview(string id, ReviewRequest? request);
    public Task<IResult> AnswerOnReview(string userId, AnswerOnReviewRequest? request);
    public Task<IResult> RateReview(Guid id, string action, string userId);
    public Task<IResult> RemoveReview(Guid? id );
}