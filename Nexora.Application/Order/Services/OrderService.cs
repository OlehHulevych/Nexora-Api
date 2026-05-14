using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;
using Nexora.Domain.Mappers;

namespace Nexora.Application.Order.Services;

public class OrderService:IOrderService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public OrderService(ICartRepository cartRepository, IOrderRepository orderRepository, IUserRepository userRepository)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }
    public async Task<IResult> AddOrder(string id)
    {
        Domain.Entities.Cart? cart = await _cartRepository.GetCartByUserId(id);
        if (cart == null) throw new NotFoundException(nameof(Domain.Entities.Cart),id);
        ApplicationUser? user = await _userRepository.FindById(id);
        if (user == null || user.Address==null) throw new NotFoundException(nameof(ApplicationUser), id);
        Domain.Entities.Order newOrder = new Domain.Entities.Order()
        {
            Buyer = user,
            BuyerId = user.Id,
            Status = OrderStatus.Pending,
            DeliveredAddress = user.Address,
            DeliveredAddressId = user.Address.Id,
        };
        if (!cart.items.Any()) throw new ArgumentException();
        List<OrderItem> items = cart.items.Select(item=> new OrderItem
        {
            ProductId = item.ListingId,
            Product = item.Listing,
            Order = newOrder,
            OrderId = newOrder.Id,
            Quantity = item.Quantity,
            UnitPrice = item.Listing.Price*item.Price
        }).ToList();
        newOrder.Items = items;
        bool result = await _orderRepository.CreateOrder(newOrder);
        OrderDTO orderDto = OrderMapper.ToDto(newOrder);
        if (!result) return Results.BadRequest(new {message = "Failed to create order", order = orderDto});
        return Results.Ok(new {message="Your order was created", order = orderDto});
    }

    public async Task<IResult> ChangeOrderStatus(Guid id, OrderStatus status)
    {
        Domain.Entities.Order order = await  _orderRepository.GetOrderById(id);
        order.Status = status;
        bool result =await _orderRepository.UpdateOrder(order);
        if (!result) return Results.BadRequest(new {messaage = "Failed to update order"});
        OrderDTO dto = OrderMapper.ToDto(order);
        return Results.Ok(new {message = "The status of order was updated", order = dto});

    }
}