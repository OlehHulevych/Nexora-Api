using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Review.Request;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/review")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddReview([FromForm] ReviewRequest request)
    {
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _reviewService.AddReview(id, request);
    }

    [Authorize]
    [HttpPost("/answer")]
    public async Task<IResult> AnswerReview([FromForm]AnswerOnReviewRequest request)
    {
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _reviewService.AnswerOnReview(id, request);
    }
    
    
    
}