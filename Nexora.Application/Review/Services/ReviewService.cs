using System.Security.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Review.Request;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Review.Services;

public class ReviewService:IReviewService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProductRepository _productRepository;
    private readonly IReviewRepository _reviewRepository;

    public ReviewService( UserManager<ApplicationUser> userManager, IProductRepository productRepository, IReviewRepository reviewRepository)
    {
        _userManager = userManager;
        _productRepository = productRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<IResult> AddReview(ReviewRequest? request)
    {
        if (request == null) throw new BadHttpRequestException("The required data are missing");
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new AuthenticationException("Failed to find user");
        var listing = await _productRepository.GetProductById(request.ListingId);
        if (listing == null) throw new NotFoundException(nameof(Listing), request.ListingId);
        Domain.Entities.Review newReview = new Domain.Entities.Review()
        {
            Author = user,
            AuthorId = user.Id,
            Product = listing,
            ProductId = listing.Id,
            Comment = request.Comment,
            Rating = request.Rating
        };
        var response = await _reviewRepository.CreateAsync(newReview);
        if (response) return Results.Ok(new {message = "The review was created", review = new ReviewDto(user.FirstName + " "+user.LastName, request.Rating, request.Comment )});
        return Results.BadRequest("Failed to create review");

    }
    

    public async Task<IResult> AnswerOnReview(AnswerOnReviewRequest? request)
    {
        if (request == null || request.ReviewId == null || request.UserId==null || request.ListingId == null) throw new ArgumentNullException(nameof(request), "ReviewId is required");
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new AuthenticationException("Failed to find user");
        Domain.Entities.Review? review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        var listing = await _productRepository.GetProductById(request.ListingId);
        if (listing != null)
        {
            Domain.Entities.Review answeringReview = new Domain.Entities.Review
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

    public async Task<IResult> RemoveReview(Guid? id)
    {
        if (id == null) throw new BadHttpRequestException("Id is required");
        var result = await  _reviewRepository.DeleteAsync(id);
        if (!result) return Results.BadRequest("Failed to delete review");
        return Results.Ok(new {message = "The review was deleted"});

    }
}