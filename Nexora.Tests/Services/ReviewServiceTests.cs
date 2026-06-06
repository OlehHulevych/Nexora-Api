using System.Security.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Reviews.Request;
using Nexora.Application.Reviews.Services;
using Nexora.Domain.Constants;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;
using Nexora.Domain.Exceptions;

namespace Nexora.Tests.Services;

[TestFixture]
public class ReviewServiceTests
{
    private Mock<IUserRepository> _userRepoMock = null!;
    private Mock<IProductRepository> _productRepoMock = null!;
    private Mock<IReviewRepository> _reviewRepoMock = null!;
    private Mock<IReviewLikeRepository> _likeRepoMock = null!;
    private Mock<ILogger<ReviewService>> _loggerMock = null!;
    private ReviewService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _reviewRepoMock = new Mock<IReviewRepository>();
        _likeRepoMock = new Mock<IReviewLikeRepository>();
        _loggerMock = new Mock<ILogger<ReviewService>>();

        _sut = new ReviewService(
            _userRepoMock.Object,
            _productRepoMock.Object,
            _reviewRepoMock.Object,
            _likeRepoMock.Object,
            _loggerMock.Object);
    }

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? 200;


    [Test]
    public void AddReview_ThrowsBadHttpRequest_WhenRequestIsNull()
    {
        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.AddReview("user-1", null));
    }

    [Test]
    public void AddReview_ThrowsAuthenticationException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<AuthenticationException>(async () =>
            await _sut.AddReview("user-1", new ReviewRequest(Guid.NewGuid(), 5, "Great!")));
    }

    [Test]
    public void AddReview_ThrowsNotFoundException_WhenListingNotFound()
    {
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", FirstName = "John", LastName = "Doe" });
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync((Listing?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.AddReview("user-1", new ReviewRequest(Guid.NewGuid(), 5, "Great!")));
    }

    [Test]
    public async Task AddReview_Returns400_WhenCreateFails()
    {
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", FirstName = "John", LastName = "Doe" });
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync(new Listing { Id = Guid.NewGuid(), Name = "iPhone", SellerId = "s1" });
        _reviewRepoMock.Setup(r => r.CreateAsync(It.IsAny<Review>())).ReturnsAsync(false);

        var result = await _sut.AddReview("user-1", new ReviewRequest(Guid.NewGuid(), 5, "Great!"));

        GetStatusCode(result).Should().Be(400);
    }

    [Test]
    public async Task AddReview_Returns200_WhenSuccessful()
    {
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", FirstName = "John", LastName = "Doe" });
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync(new Listing { Id = Guid.NewGuid(), Name = "iPhone", SellerId = "s1" });
        _reviewRepoMock.Setup(r => r.CreateAsync(It.IsAny<Review>())).ReturnsAsync(true);

        var result = await _sut.AddReview("user-1", new ReviewRequest(Guid.NewGuid(), 5, "Great!"));

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public void RateReview_ThrowsNotFoundException_WhenReviewNotFound()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid?>()))
            .ReturnsAsync((Review?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.RateReview(new RateReviewRequest(Guid.NewGuid(), LikeNames.like), "user-1"));
    }

    [Test]
    public void RateReview_ThrowsNotFoundException_WhenUserNotFound()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(new Review { AuthorId = "author-1", Likes = [] });
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.RateReview(new RateReviewRequest(Guid.NewGuid(), LikeNames.like), "user-1"));
    }

    [Test]
    public async Task RateReview_TogglesLikeToDislike_WhenLikeAlreadyExists()
    {
        var reviewId = Guid.NewGuid();
        var user = new ApplicationUser { Id = "user-1" };
        var review = new Review { AuthorId = "author-1", Likes = [] };

        // ReviewLike requires Review and Author
        var existingLike = new ReviewLike
        {
            ReviewId = reviewId,
            Review = review,
            AuthorId = "user-1",
            Author = user,
            Act = ReviewActs.LIKE
        };

        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid?>())).ReturnsAsync(review);
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>())).ReturnsAsync(user);
        _likeRepoMock.Setup(r => r.Exists(It.IsAny<ReviewLike>())).ReturnsAsync(true);
        _likeRepoMock.Setup(r => r.GetByReviewIdAndUserId(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(existingLike);
        _likeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ReviewLike>())).ReturnsAsync(true);

        var result = await _sut.RateReview(new RateReviewRequest(reviewId, LikeNames.like), "user-1");

        GetStatusCode(result).Should().Be(200);
        existingLike.Act.Should().Be(ReviewActs.DISLIKE);
    }

    [Test]
    public async Task RateReview_TogglesDislikeToLike_WhenDislikeAlreadyExists()
    {
        var reviewId = Guid.NewGuid();
        var user = new ApplicationUser { Id = "user-1" };
        var review = new Review { AuthorId = "author-1", Likes = [] };

        var existingLike = new ReviewLike
        {
            ReviewId = reviewId,
            Review = review,
            AuthorId = "user-1",
            Author = user,
            Act = ReviewActs.DISLIKE
        };

        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid?>())).ReturnsAsync(review);
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>())).ReturnsAsync(user);
        _likeRepoMock.Setup(r => r.Exists(It.IsAny<ReviewLike>())).ReturnsAsync(true);
        _likeRepoMock.Setup(r => r.GetByReviewIdAndUserId(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(existingLike);
        _likeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ReviewLike>())).ReturnsAsync(true);

        var result = await _sut.RateReview(new RateReviewRequest(reviewId, LikeNames.dislike), "user-1");

        GetStatusCode(result).Should().Be(200);
        existingLike.Act.Should().Be(ReviewActs.LIKE);
    }

    [Test]
    public async Task RateReview_CreatesNewLike_WhenNoneExists()
    {
        var reviewId = Guid.NewGuid();
        var review = new Review { AuthorId = "author-1", Likes = [] };

        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid?>())).ReturnsAsync(review);
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1" });
        _likeRepoMock.Setup(r => r.Exists(It.IsAny<ReviewLike>())).ReturnsAsync(false);
        _likeRepoMock.Setup(r => r.Create(It.IsAny<ReviewLike>())).ReturnsAsync(true);
        _reviewRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Review>())).ReturnsAsync(true);

        var result = await _sut.RateReview(new RateReviewRequest(reviewId, LikeNames.like), "user-1");

        GetStatusCode(result).Should().Be(200);
        _likeRepoMock.Verify(r => r.Create(It.IsAny<ReviewLike>()), Times.Once);
    }


    [Test]
    public void RemoveReview_ThrowsBadHttpRequest_WhenIdIsNull()
    {
        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.RemoveReview(null));
    }

    [Test]
    public async Task RemoveReview_Returns400_WhenDeleteFails()
    {
        _reviewRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid?>())).ReturnsAsync(false);

        var result = await _sut.RemoveReview(Guid.NewGuid());

        GetStatusCode(result).Should().Be(400);
    }

    [Test]
    public async Task RemoveReview_Returns200_WhenSuccessful()
    {
        _reviewRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid?>())).ReturnsAsync(true);

        var result = await _sut.RemoveReview(Guid.NewGuid());

        GetStatusCode(result).Should().Be(200);
    }
}
