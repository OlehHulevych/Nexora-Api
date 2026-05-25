using System.Security.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Reviews.Request;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Enums;
using Nexora.Domain.Exceptions;
using Nexora.Domain.Mappers;

namespace Nexora.Application.Reviews.Services;

public class ReviewService : IReviewService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IReviewLikeRepository _reviewLikeRepository;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IUserRepository userRepository, IProductRepository productRepository,
        IReviewRepository reviewRepository, IReviewLikeRepository reviewLikeRepository,
        ILogger<ReviewService> logger)
    {
        _productRepository = productRepository;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _reviewLikeRepository = reviewLikeRepository;
        _logger = logger;
    }

    public async Task<IResult> AddReview(string id, ReviewRequest? request)
    {
        _logger.LogInformation("Adding review by user {UserId}", id);

        if (request == null)
        {
            _logger.LogWarning("Add review failed — request is null for user {UserId}", id);
            throw new BadHttpRequestException("The required data are missing");
        }

        var user = await _userRepository.FindById(id);
        if (user == null)
        {
            _logger.LogWarning("Add review failed — user {UserId} not found", id);
            throw new AuthenticationException("Failed to find user");
        }

        var listing = await _productRepository.GetById(request.ListingId);
        if (listing == null)
        {
            _logger.LogWarning("Add review failed — listing {ListingId} not found", request.ListingId);
            throw new NotFoundException(nameof(Listing), request.ListingId);
        }

        Review newReview = new Review()
        {
            Author = user,
            AuthorId = user.Id,
            Product = listing,
            ProductId = listing.Id,
            Comment = request.Comment,
            Rating = request.Rating,
            Likes = new List<ReviewLike>()
        };

        var response = await _reviewRepository.CreateAsync(newReview);
        if (response)
        {
            _logger.LogInformation("Review created successfully by user {UserId} for listing {ListingId}", id, request.ListingId);
            return Results.Ok(new { message = "The review was created", review = new ReviewDto(user.FirstName + " " + user.LastName, request.Rating, request.Comment) });
        }

        _logger.LogError("Failed to create review by user {UserId} for listing {ListingId}", id, request.ListingId);
        return Results.BadRequest("Failed to create review");
    }

    public async Task<IResult> AnswerOnReview(string userId, AnswerOnReviewRequest? request)
    {
        _logger.LogInformation("User {UserId} answering on review {ReviewId}", userId, request?.ReviewId);

        if (request == null || request.ReviewId == null || request.ListingId == null)
        {
            _logger.LogWarning("Answer on review failed — missing required fields for user {UserId}", userId);
            throw new ArgumentNullException(nameof(request), "ReviewId is required");
        }

        var user = await _userRepository.FindById(userId);
        if (user == null)
        {
            _logger.LogWarning("Answer on review failed — user {UserId} not found", userId);
            throw new AuthenticationException("Failed to find user");
        }

        Review? review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
        {
            _logger.LogWarning("Answer on review failed — review {ReviewId} not found", request.ReviewId);
        }

        var listing = await _productRepository.GetById(request.ListingId);
        if (listing != null)
        {
            Review answeringReview = new Review
            {
                Author = user,
                MainReviewId = review?.Id,
                MainReview = review,
                AuthorId = user.Id,
                Product = listing,
                ProductId = listing.Id,
                Comment = request.Comment,
            };

            await _reviewRepository.CreateAsync(answeringReview);
            _logger.LogInformation("Answer review created by user {UserId} for review {ReviewId}", userId, request.ReviewId);
            review?.Reviews.Add(answeringReview);
        }
        else
        {
            _logger.LogWarning("Answer on review failed — listing {ListingId} not found", request.ListingId);
        }

        var result = review != null && await _reviewRepository.UpdateAsync(review);
        if (!result)
        {
            _logger.LogError("Failed to update review {ReviewId} with answer from user {UserId}", request.ReviewId, userId);
            throw new BadHttpRequestException("Failed to create answer on review");
        }

        _logger.LogInformation("Answer on review {ReviewId} created successfully by user {UserId}", request.ReviewId, userId);
        return Results.Ok(new { message = "The answer was created" });
    }

    public async Task<IResult> RateReview(RateReviewRequest request, string userId)
    {
        _logger.LogInformation("User {UserId} rating review {ReviewId} with action {Action}", userId, request.ReviewId, request.Action);

        Review? review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
        {
            _logger.LogWarning("Rate review failed — review {ReviewId} not found", request.ReviewId);
            throw new NotFoundException(nameof(Review), request.ReviewId);
        }

        ApplicationUser? author = await _userRepository.FindById(userId);
        if (author == null)
        {
            _logger.LogWarning("Rate review failed — user {UserId} not found", userId);
            throw new NotFoundException(nameof(ApplicationUser), userId);
        }

        ReviewLike newReviewLike = new ReviewLike
        {
            ReviewId = review.Id,
            Review = review,
            AuthorId = userId,
            Author = author,
        };

        bool existLike = await _reviewLikeRepository.Exists(newReviewLike);
        if (existLike)
        {
            _logger.LogInformation("Existing like found for user {UserId} on review {ReviewId} — toggling", userId, request.ReviewId);

            ReviewLike? like = await _reviewLikeRepository.GetByReviewIdAndUserId(newReviewLike.ReviewId, newReviewLike.AuthorId);
            if (like == null)
            {
                _logger.LogWarning("Like not found for user {UserId} on review {ReviewId}", userId, request.ReviewId);
                throw new NotFoundException(nameof(ReviewLike), newReviewLike.ReviewId);
            }

            if (like.Act == ReviewActs.LIKE) like.Act = ReviewActs.DISLIKE;
            else if (like.Act == ReviewActs.DISLIKE) like.Act = ReviewActs.LIKE;

            await _reviewLikeRepository.UpdateAsync(like);
            _logger.LogInformation("Like toggled to {Act} for user {UserId} on review {ReviewId}", like.Act, userId, request.ReviewId);
            return Results.Ok(new { message = "The like was updated" });
        }

        if (request.Action == LikeNames.like) newReviewLike.Act = ReviewActs.LIKE;
        else if (request.Action == LikeNames.dislike) newReviewLike.Act = ReviewActs.DISLIKE;

        await _reviewLikeRepository.Create(newReviewLike);
        _logger.LogInformation("New {Act} created by user {UserId} on review {ReviewId}", newReviewLike.Act, userId, request.ReviewId);

        ReviewLikeDto dto = ReviewLikeMapper.ToDto(newReviewLike);
        review.Likes.Add(newReviewLike);
        await _reviewRepository.UpdateAsync(review);

        return Results.Ok(new { message = "your rating was sent", data = dto });
    }

    public async Task<IResult> RemoveReview(Guid? id)
    {
        _logger.LogInformation("Removing review {ReviewId}", id);

        if (id == null)
        {
            _logger.LogWarning("Remove review failed — id is null");
            throw new BadHttpRequestException("Id is required");
        }

        var result = await _reviewRepository.DeleteAsync(id);
        if (!result)
        {
            _logger.LogError("Failed to delete review {ReviewId}", id);
            return Results.BadRequest("Failed to delete review");
        }

        _logger.LogInformation("Review {ReviewId} deleted successfully", id);
        return Results.Ok(new { message = "The review was deleted" });
    }
}