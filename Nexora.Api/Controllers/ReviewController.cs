using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Reviews.Request;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/review")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }
    /// <summary>
    /// Creating review on listing
    /// </summary>
    /// <param name="request">contain id of listing, description and rating</param>
    /// <returns>created review</returns>
    /// <response code="200">review was created successfully</response>
    /// <response code="404">listing is not existing</response>
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddReview([FromForm] ReviewRequest request)
    {
        _logger.LogInformation("Review controller - creating review according to listing id");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            _logger.LogError("Review Controller - user is unauthorized");
            throw new UnauthorizedAccessException();
        }
        return await _reviewService.AddReview(id, request);
    }
    /// <summary>
    /// Answering on existing review
    /// </summary>
    /// <param name="request">contain id of listing, review id and answer itself</param>
    /// <returns>created answer</returns>
    /// <response code="200">answer was created successfully</response>
    /// <response code="404">listing is not existing</response>
    [Authorize]
    [HttpPost("/answer")]
    public async Task<IResult> AnswerReview([FromForm]AnswerOnReviewRequest request)
    {
        _logger.LogInformation("Review controller - answering on existing review");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            _logger.LogError("Review Controller - user is unauthorized");
            throw new UnauthorizedAccessException();
        }
        return await _reviewService.AnswerOnReview(id, request);
    }
    /// <summary>
    /// Liking or disliking review
    /// </summary>
    /// <param name="request">contain id of review and action</param>
    /// <returns>created like</returns>
    /// <response code="200">like was created successfully</response>
    /// <response code="404">review is not existing</response>
    [Authorize]
    [HttpPost("/like")]
    public async Task<IResult> LikeReview([FromForm] RateReviewRequest request)
    {
        _logger.LogInformation("Review controller - rating existing review");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            _logger.LogError("Review Controller - user is unauthorized");
            throw new UnauthorizedAccessException();
        }
        return await _reviewService.RateReview(request, id);
    } 
    
    
    
    
}