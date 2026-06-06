using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nexora.Application.Favorites.Services;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Tests.Services;

[TestFixture]
public class FavoriteServiceTests
{
    private Mock<IFavoriteListRepository> _listRepoMock = null!;
    private Mock<IBaseRepository<FavoriteItem, Guid>> _itemRepoMock = null!;
    private Mock<IProductRepository> _productRepoMock = null!;
    private Mock<ILogger<FavoriteService>> _loggerMock = null!;
    private FavoriteService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _listRepoMock = new Mock<IFavoriteListRepository>();
        _itemRepoMock = new Mock<IBaseRepository<FavoriteItem, Guid>>();
        _productRepoMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<FavoriteService>>();

        _sut = new FavoriteService(
            _listRepoMock.Object,
            _itemRepoMock.Object,
            _productRepoMock.Object,
            _loggerMock.Object);
    }

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? 200;

    private static FavoriteList MakeFavoriteList(string userId = "user-1", List<FavoriteItem>? items = null)
    {
        var user = new ApplicationUser { Id = userId };
        return new FavoriteList
        {
            UserId = userId,
            User = user,
            FavoriteItems = items ?? []
        };
    }


    [Test]
    public void AddItemToList_ThrowsNotFoundException_WhenListNotFound()
    {
        _listRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync((FavoriteList?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.AddItemToList("user-1", Guid.NewGuid()));
    }

    [Test]
    public void AddItemToList_ThrowsNotFoundException_WhenListingNotFound()
    {
        _listRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(MakeFavoriteList());
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync((Listing?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.AddItemToList("user-1", Guid.NewGuid()));
    }

    [Test]
    public void AddItemToList_ThrowsConflictException_WhenAlreadyInFavorites()
    {
        var listingId = Guid.NewGuid();
        _listRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(MakeFavoriteList(items: [new FavoriteItem { ListingId = listingId }]));
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync(new Listing { Id = listingId, Name = "iPhone", SellerId = "seller-1" });

        Assert.ThrowsAsync<ConflictException>(async () =>
            await _sut.AddItemToList("user-1", listingId));
    }

    [Test]
    public async Task AddItemToList_Returns200_WhenSuccessful()
    {
        var listingId = Guid.NewGuid();
        _listRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(MakeFavoriteList());
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync(new Listing { Id = listingId, Name = "iPhone", Price = 999, SellerId = "seller-1" });
        _itemRepoMock.Setup(r => r.Add(It.IsAny<FavoriteItem>())).ReturnsAsync(true);

        var result = await _sut.AddItemToList("user-1", listingId);

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public void GetFavoriteList_ThrowsNotFoundException_WhenListNotFound()
    {
        _listRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync((FavoriteList?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.GetFavoriteList("user-1"));
    }

    [Test]
    public async Task GetFavoriteList_Returns200_WhenEmpty()
    {
        _listRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(MakeFavoriteList());

        var result = await _sut.GetFavoriteList("user-1");

        GetStatusCode(result).Should().Be(200);
    }

    [Test]
    public async Task GetFavoriteList_Returns200_WithItems()
    {
        var items = new List<FavoriteItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ListingId = Guid.NewGuid(),
                Listing = new Listing { Name = "iPhone", Price = 999, SellerId = "seller-1" }
            }
        };
        _listRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(MakeFavoriteList(items: items));

        var result = await _sut.GetFavoriteList("user-1");

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public async Task DeleteItemFromList_Returns400_WhenDeleteFails()
    {
        // IBaseRepository<FavoriteItem, Guid>.Delete(Guid id) — Guid not Guid? (unconstrained generic)
        _itemRepoMock.Setup(r => r.Delete(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _sut.DeleteItemFromList(Guid.NewGuid());

        GetStatusCode(result).Should().Be(400);
    }

    [Test]
    public async Task DeleteItemFromList_Returns200_WhenSuccessful()
    {
        _itemRepoMock.Setup(r => r.Delete(It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await _sut.DeleteItemFromList(Guid.NewGuid());

        GetStatusCode(result).Should().Be(200);
    }
}
