using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Review.Request;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Review.Services;

public class ReviewService:IReviewService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewService(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ReviewDto?> AddReview(ReviewRequest? request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new AuthenticationException("Failed to find user");
        var listing = await _context.Listings.FirstOrDefaultAsync(l=>l.Id.Equals(request.ListingId));
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
        await _context.Reviews.AddAsync(newReview);
        await _context.SaveChangesAsync();
        return new ReviewDto(user.FirstName + " "+user.LastName, request.Rating, request.Comment );

    }

    public async Task<Guid?> LikeReview(Guid? reviewId)
    {
        if (reviewId == null) throw new ArgumentNullException(nameof(reviewId), "ReviewId is required");
        Domain.Entities.Review? review = await _context.Reviews.FirstOrDefaultAsync(r=>r.Id.Equals(reviewId));
        if (review == null) throw new NullReferenceException(nameof(Domain.Entities.Review));
        review.Likes += 1;
        await _context.SaveChangesAsync();
        return review.Id;
    }

    public async Task<ReviewDto?> AnswerOnReview(AnswerOnReviewRequest? request)
    {
        if (request == null || request.ReviewId == null || request.UserId==null || request.ListingId == null) throw new ArgumentNullException(nameof(request), "ReviewId is required");
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new AuthenticationException("Failed to find user");
        var review = await _context.Reviews.FirstOrDefaultAsync(r=>r.Id.Equals(request.ReviewId));
        if(review == null) throw new NotFoundException(nameof(review), request.ReviewId);
        var listing = await _context.Listings.FirstOrDefaultAsync(l=>l.Id.Equals(request.ListingId));
        if (listing == null) throw new NotFoundException(nameof(Listing), request.ListingId);
        Domain.Entities.Review answeringReview = new Domain.Entities.Review
        {
            Author = user,
            MainReviewId = review.Id,
            MainReview = review,
            AuthorId = user.Id,
            Product = listing,
            ProductId = listing.Id,
            Comment = request.Comment,
           
        };

        await _context.Reviews.AddAsync(review);
        review.Reviews.Add(answeringReview);
        await _context.SaveChangesAsync();
        return new ReviewDto(user.FirstName + " "+user.LastName, null, request.Comment );


    }
}