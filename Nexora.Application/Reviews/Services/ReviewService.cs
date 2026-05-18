using System.Security.Authentication;
using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Reviews.Request;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Enums;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Reviews.Services;

public class ReviewService:IReviewService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IUserRepository userRepository , IProductRepository productRepository, IReviewRepository reviewRepository)
    {
        _productRepository = productRepository;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
    }

    public async Task<IResult> AddReview(string id, ReviewRequest? request)
    {
        if (request == null) throw new BadHttpRequestException("The required data are missing");
        var user = await _userRepository.FindById(id);
        if (user == null) throw new AuthenticationException("Failed to find user");
        var listing = await _productRepository.GetById(request.ListingId);
        if (listing == null) throw new NotFoundException(nameof(Listing), request.ListingId);
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
        if (response) return Results.Ok(new {message = "The review was created", review = new ReviewDto(user.FirstName + " "+user.LastName, request.Rating, request.Comment )});
        return Results.BadRequest("Failed to create review");

    }
    

    public async Task<IResult> AnswerOnReview(string userId,AnswerOnReviewRequest? request)
    {
        if (request == null || request.ReviewId == null || request.ListingId == null) throw new ArgumentNullException(nameof(request), "ReviewId is required");
        var user = await _userRepository.FindById(userId);
        if (user == null) throw new AuthenticationException("Failed to find user");
        Review? review = await _reviewRepository.GetByIdAsync(request.ReviewId);
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
            review?.Reviews.Add(answeringReview);
        }

        var result = review != null && await  _reviewRepository.UpdateAsync(review);
        if (!result) throw new BadHttpRequestException("Failed to create answer on review");
        return Results.Ok(new {message = "The answer was created"});


    }

    public async Task<IResult> RateReview(Guid id,string action, string userId)
    {
        Review? review = await _reviewRepository.GetByIdAsync(id);
        if (review == null) throw new NotFoundException(nameof(Review), id);
        ApplicationUser? author = await _userRepository.FindById(userId);
        if (author == null) throw new NotFoundException(nameof(ApplicationUser), userId);
        ReviewLike newReviewLike = new ReviewLike
        {
            ReviewId = review.Id,
            Review = review,
            AuthorId = userId,
            Author = author,
        };
        if (action == LikeNames.like) newReviewLike.Act = ReviewActs.LIKE;
        else if (action == LikeNames.dislike) newReviewLike.Act = ReviewActs.DISLIKE;
    }

    public async Task<IResult> RemoveReview(Guid? id)
    {
        if (id == null) throw new BadHttpRequestException("Id is required");
        var result = await  _reviewRepository.DeleteAsync(id);
        if (!result) return Results.BadRequest("Failed to delete review");
        return Results.Ok(new {message = "The review was deleted"});

    }
}