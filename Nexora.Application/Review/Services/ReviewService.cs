using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Review.Request;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Review.Services;

public class ReviewService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewService(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ReviewDto> AddReview(ReviewRequest? request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new AuthenticationException("Failed to fetch user");
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
}