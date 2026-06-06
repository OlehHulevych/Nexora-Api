using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Services;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Tests.Services;

[TestFixture]
public class ListingServiceTests
{
    private Mock<IUserRepository> _userRepoMock = null!;
    private Mock<ICategoryRepository> _categoryRepoMock = null!;
    private Mock<IProductBlobStorage> _storageMock = null!;
    private Mock<IProductRepository> _productRepoMock = null!;
    private Mock<ILogger<ListingService>> _loggerMock = null!;
    private ListingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _storageMock = new Mock<IProductBlobStorage>();
        _productRepoMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ListingService>>();

        _sut = new ListingService(
            _userRepoMock.Object,
            _categoryRepoMock.Object,
            _storageMock.Object,
            _productRepoMock.Object,
            _loggerMock.Object);
    }

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? 200;


    [Test]
    public void AddProduct_ThrowsBadHttpRequest_WhenCategoryNotFound()
    {
        _categoryRepoMock.Setup(r => r.GetCategory(It.IsAny<string>()))
            .ReturnsAsync((Category?)null);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.AddProduct(new CreateProductCommand("iPhone", "desc", 999, 10, "Electronics", []), "user-1"));
    }

    [Test]
    public void AddProduct_ThrowsBadHttpRequest_WhenUserNotFound()
    {
        _categoryRepoMock.Setup(r => r.GetCategory(It.IsAny<string>()))
            .ReturnsAsync(new Category("Electronics"));
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.AddProduct(new CreateProductCommand("iPhone", "desc", 999, 10, "Electronics", []), "user-1"));
    }

    [Test]
    public void AddProduct_ThrowsException_WhenDbCreateReturnsEmpty()
    {
        _categoryRepoMock.Setup(r => r.GetCategory(It.IsAny<string>()))
            .ReturnsAsync(new Category("Electronics"));
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1" });
        _productRepoMock.Setup(r => r.Create(It.IsAny<Listing>()))
            .ReturnsAsync((Guid?)Guid.Empty);

        Assert.ThrowsAsync<Exception>(async () =>
            await _sut.AddProduct(new CreateProductCommand("iPhone", "desc", 999, 10, "Electronics", []), "user-1"));
    }

    [Test]
    public void AddProduct_ThrowsException_WhenPhotoUploadReturnsEmpty()
    {
        _categoryRepoMock.Setup(r => r.GetCategory(It.IsAny<string>()))
            .ReturnsAsync(new Category("Electronics"));
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1" });
        _productRepoMock.Setup(r => r.Create(It.IsAny<Listing>()))
            .ReturnsAsync((Guid?)Guid.NewGuid());
        // Must pass CancellationToken explicitly to avoid CS0854 (optional param in expression tree)
        _storageMock.Setup(r => r.UploadAsync(
                It.IsAny<List<IFormFile>>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<Listing>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductImage>());

        Assert.ThrowsAsync<Exception>(async () =>
            await _sut.AddProduct(
                new CreateProductCommand("iPhone", "desc", 999, 10, "Electronics", [new Mock<IFormFile>().Object]),
                "user-1"));
    }

    [Test]
    public async Task AddProduct_Returns200_WhenSuccessful()
    {
        var productId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetCategory(It.IsAny<string>()))
            .ReturnsAsync(new Category("Electronics"));
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1" });
        _productRepoMock.Setup(r => r.Create(It.IsAny<Listing>()))
            .ReturnsAsync((Guid?)Guid.NewGuid());
        _storageMock.Setup(r => r.UploadAsync(
                It.IsAny<List<IFormFile>>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<Listing>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductImage> { new(productId, "http://blob/img.jpg", "blob/img.jpg") });

        var result = await _sut.AddProduct(
            new CreateProductCommand("iPhone", "desc", 999, 10, "Electronics", [new Mock<IFormFile>().Object]),
            "user-1");

        GetStatusCode(result).Should().Be(200);
    }

    
    [Test]
    public void GetProductsService_ThrowsBadHttpRequest_WhenRepoReturnsNull()
    {
        _productRepoMock.Setup(r => r.GetAll(It.IsAny<GetProductsCommand>()))
            .ReturnsAsync((IQueryable<Listing>?)null);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.GetProductsService(new GetProductsCommand(null, null)));
    }

    [Test]
    public void GetProductsService_ThrowsBadHttpRequest_WhenNoListingsExist()
    {
        var mockQ = new List<Listing>().AsQueryable().BuildMock();
        _productRepoMock.Setup(r => r.GetAll(It.IsAny<GetProductsCommand>()))
            .ReturnsAsync(mockQ.Object);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.GetProductsService(new GetProductsCommand(null, null)));
    }

    [Test]
    public async Task GetProductsService_Returns200_WhenListingsFound()
    {
        var listings = new List<Listing>
        {
            new()
            {
                Name = "iPhone", Description = "Latest model", Price = 999,
                StockQuantity = 5, isActive = true, SellerId = "s1",
                CreatedAt = DateTime.UtcNow,
                Category = new Category("Electronics"),
                Images = new List<ProductImage>(),
                Reviews = new List<Review>()
            }
        };
        var mockQ = listings.AsQueryable().BuildMock();
        _productRepoMock.Setup(r => r.GetAll(It.IsAny<GetProductsCommand>()))
            .ReturnsAsync(mockQ.Object);

        var result = await _sut.GetProductsService(new GetProductsCommand(null, null));

        GetStatusCode(result).Should().Be(200);
    }

    [TestCase(null, "electronics")]   // service does .ToLower() on search — data must match lowercase
    [TestCase("iPhone", null)]
    [TestCase("iPhone", "electronics")]
    public async Task GetProductsService_Returns200_WithSearchFilters(string? name, string? category)
    {
        var listings = new List<Listing>
        {
            new()
            {
                Name = "iPhone", SellerId = "s1", CreatedAt = DateTime.UtcNow,
                Category = new Category("electronics"), // lowercase to match Contains(category.ToLower())
                Images = new List<ProductImage>(),
                Reviews = new List<Review>()
            }
        };
        var mockQ = listings.AsQueryable().BuildMock();
        _productRepoMock.Setup(r => r.GetAll(It.IsAny<GetProductsCommand>()))
            .ReturnsAsync(mockQ.Object);

        var result = await _sut.GetProductsService(new GetProductsCommand(name, category));

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public void GetProductById_ThrowsBadHttpRequest_WhenNotFound()
    {
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync((Listing?)null);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.GetProductById(Guid.NewGuid()));
    }

    [Test]
    public async Task GetProductById_Returns200_WhenFound()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync(new Listing
            {
                Id = id, Name = "iPhone", Price = 999, StockQuantity = 10,
                isActive = true, SellerId = "s1",
                Category = new Category("Electronics"),
                Images = new List<ProductImage>(),
                Reviews = new List<Review>()
            });

        var result = await _sut.GetProductById(id);

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public void UpdateProductHandler_ThrowsNotFoundException_WhenListingNotFound()
    {
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync((Listing?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.UpdateProductHandler(
                new UpdateProductCommand(Guid.NewGuid(), null, null, null, null, null, null, null)));
    }

    [Test]
    public void UpdateProductHandler_ThrowsBlobStorageException_WhenPhotoDeleteFails()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync(new Listing { Id = id, Name = "iPhone", SellerId = "s1", Images = [], Reviews = [] });
        _productRepoMock.Setup(r => r.Update(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);
        _storageMock.Setup(r => r.DeleteForEditing(It.IsAny<ICollection<string>>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<BlobStorageException>(async () =>
            await _sut.UpdateProductHandler(
                new UpdateProductCommand(id, null, null, null, null, null, null, ["old-photo.jpg"])));
    }

    [Test]
    public async Task UpdateProductHandler_Returns200_WhenSuccessful()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetById(It.IsAny<Guid?>()))
            .ReturnsAsync(new Listing { Id = id, Name = "Old", SellerId = "s1", Images = [], Reviews = [] });
        _productRepoMock.Setup(r => r.Update(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateProductHandler(
            new UpdateProductCommand(id, "New Name", "desc", 500m, 10, null, null, null));

        GetStatusCode(result).Should().Be(200);
    }
}
