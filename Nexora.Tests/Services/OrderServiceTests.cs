using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Orders.Services;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Tests.Services;

[TestFixture]
public class OrderServiceTests
{
    private Mock<ICartRepository> _cartRepoMock = null!;
    private Mock<IOrderRepository> _orderRepoMock = null!;
    private Mock<IUserRepository> _userRepoMock = null!;
    private Mock<ILogger<OrderService>> _loggerMock = null!;
    private OrderService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<OrderService>>();

        _sut = new OrderService(
            _cartRepoMock.Object,
            _orderRepoMock.Object,
            _userRepoMock.Object,
            _loggerMock.Object);
    }

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? 200;

    private static Order MakeValidOrder(Guid? id = null, OrderStatus status = OrderStatus.Pending)
    {
        var address = new Address("buyer-1", "123 St", "NY", "US", "10001", null);
        var listing = new Listing { Name = "iPhone", SellerId = "seller-1" };
        return new Order
        {
            Id = id ?? Guid.NewGuid(),
            Status = status,
            BuyerId = "buyer-1",
            DeliveredAddress = address,
            DeliveredAddressId = address.Id,
            Items = [new OrderItem { Product = listing, Quantity = 1, UnitPrice = 100 }]
        };
    }

    private static (ApplicationUser user, Address address, Listing listing) MakeOrderSetup()
    {
        var address = new Address("user-1", "123 St", "NY", "US", "10001", null);
        var listing = new Listing { Id = Guid.NewGuid(), Name = "iPhone", SellerId = "seller-1" };
        var user = new ApplicationUser { Id = "user-1", Address = address };
        return (user, address, listing);
    }


    [Test]
    public void AddOrder_ThrowsNotFoundException_WhenCartNotFound()
    {
        _cartRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync((Cart?)null);

        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.AddOrder("user-1"));
    }

    [Test]
    public void AddOrder_ThrowsArgumentException_WhenCartIsEmpty()
    {
        _cartRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(new Cart { UserId = "user-1", items = [] });

        Assert.ThrowsAsync<ArgumentException>(async () => await _sut.AddOrder("user-1"));
    }

    [Test]
    public void AddOrder_ThrowsNotFoundException_WhenUserNotFound()
    {
        _cartRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(new Cart
            {
                UserId = "user-1",
                items = [new CartItem { ListingId = Guid.NewGuid(), Quantity = 1, Price = 100 }]
            });
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.AddOrder("user-1"));
    }

    [Test]
    public void AddOrder_ThrowsNotFoundException_WhenUserHasNoAddress()
    {
        _cartRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(new Cart
            {
                UserId = "user-1",
                items = [new CartItem { Quantity = 1, Price = 100 }]
            });
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Address = null });

        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.AddOrder("user-1"));
    }

    [Test]
    public async Task AddOrder_Returns400_WhenSaveFails()
    {
        var (user, _, listing) = MakeOrderSetup();

        _cartRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(new Cart
            {
                UserId = "user-1",
                items = [new CartItem { ListingId = listing.Id, Listing = listing, Quantity = 1, Price = 100 }]
            });
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>())).ReturnsAsync(user);
        _orderRepoMock.Setup(r => r.Add(It.IsAny<Order?>())).ReturnsAsync(false);

        var result = await _sut.AddOrder("user-1");

        GetStatusCode(result).Should().Be(400);
    }

    [Test]
    public async Task AddOrder_Returns200_WhenSuccessful()
    {
        var (user, _, listing) = MakeOrderSetup();

        _cartRepoMock.Setup(r => r.GetByUserId(It.IsAny<string>()))
            .ReturnsAsync(new Cart
            {
                UserId = "user-1",
                items = [new CartItem { ListingId = listing.Id, Listing = listing, Quantity = 2, Price = 500 }]
            });
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>())).ReturnsAsync(user);
        _orderRepoMock.Setup(r => r.Add(It.IsAny<Order?>())).ReturnsAsync(true);
        _orderRepoMock.Setup(r => r.Update(It.IsAny<Order>())).ReturnsAsync(true);

        var result = await _sut.AddOrder("user-1");

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public void ChangeOrderStatus_ThrowsNotFoundException_WhenOrderNotFound()
    {
        _orderRepoMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.ChangeOrderStatus(Guid.NewGuid(), OrderStatus.Paid));
    }

    [Test]
    public async Task ChangeOrderStatus_Returns400_WhenUpdateFails()
    {
        var orderId = Guid.NewGuid();
        _orderRepoMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(MakeValidOrder(orderId));
        _orderRepoMock.Setup(r => r.Update(It.IsAny<Order>())).ReturnsAsync(false);

        var result = await _sut.ChangeOrderStatus(orderId, OrderStatus.Paid);

        GetStatusCode(result).Should().Be(400);
    }

    [Test]
    public async Task ChangeOrderStatus_Returns200_AndUpdatesStatus()
    {
        var orderId = Guid.NewGuid();
        var order = MakeValidOrder(orderId, OrderStatus.Pending);
        _orderRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(order);
        _orderRepoMock.Setup(r => r.Update(It.IsAny<Order>())).ReturnsAsync(true);

        var result = await _sut.ChangeOrderStatus(orderId, OrderStatus.Paid);

        GetStatusCode(result).Should().Be(200);
        order.Status.Should().Be(OrderStatus.Paid);
    }


    [Test]
    public async Task GetOrders_Returns200_WithEmptyList()
    {
        _orderRepoMock.Setup(r => r.GetByUser(It.IsAny<string>()))
            .ReturnsAsync([]);

        var result = await _sut.GetOrders("user-1");

        GetStatusCode(result).Should().Be(200);
    }

    [Test]
    public async Task GetOrders_Returns200_WithOrders()
    {
        _orderRepoMock.Setup(r => r.GetByUser(It.IsAny<string>()))
            .ReturnsAsync([MakeValidOrder()]);

        var result = await _sut.GetOrders("user-1");

        GetStatusCode(result).Should().Be(200);
    }
}
