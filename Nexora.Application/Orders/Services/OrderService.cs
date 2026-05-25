using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;
using Nexora.Domain.Mappers;

namespace Nexora.Application.Orders.Services;

public class OrderService : IOrderService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ICartRepository cartRepository, IOrderRepository orderRepository,
        IUserRepository userRepository, ILogger<OrderService> logger)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IResult> AddOrder(string id)
    {
        _logger.LogInformation("Creating order for user {UserId}", id);

        Cart? cart = await _cartRepository.GetByUserId(id);
        if (cart == null)
        {
            _logger.LogWarning("Add order failed — cart not found for user {UserId}", id);
            throw new NotFoundException(nameof(Cart), id);
        }

        if (!cart.items.Any())
        {
            _logger.LogWarning("Add order failed — cart is empty for user {UserId}", id);
            throw new ArgumentException("Cart is empty");
        }

        ApplicationUser? user = await _userRepository.FindById(id);
        if (user == null || user.Address == null)
        {
            _logger.LogWarning("Add order failed — user {UserId} not found or has no address", id);
            throw new NotFoundException(nameof(ApplicationUser), id);
        }

        Order newOrder = new Order()
        {
            Buyer = user,
            BuyerId = user.Id,
            Status = OrderStatus.Pending,
            DeliveredAddress = user.Address,
            DeliveredAddressId = user.Address.Id,
        };

        List<OrderItem> items = cart.items.Select(item => new OrderItem
        {
            ProductId = item.ListingId,
            Product = item.Listing,
            Order = newOrder,
            OrderId = newOrder.Id,
            Quantity = item.Quantity,
            UnitPrice = item.Quantity * item.Price
        }).ToList();

        newOrder.Items = items;
        _logger.LogInformation("Order contains {ItemCount} items for user {UserId}", items.Count, id);

        bool result = await _orderRepository.Add(newOrder);
        if (!result)
        {
            _logger.LogError("Failed to save order to database for user {UserId}", id);
            OrderDTO failedDto = OrderMapper.ToDto(newOrder);
            return Results.BadRequest(new { message = "Failed to create order", order = failedDto });
        }

        newOrder.TotalAmount = items.Sum(i => i.UnitPrice);
        await _orderRepository.Update(newOrder);

        _logger.LogInformation("Order {OrderId} created successfully for user {UserId} — total {TotalAmount}",
            newOrder.Id, id, newOrder.TotalAmount);

        OrderDTO orderDto = OrderMapper.ToDto(newOrder);
        return Results.Ok(new { message = "Your order was created", order = orderDto });
    }

    public async Task<IResult> ChangeOrderStatus(Guid id, OrderStatus status)
    {
        _logger.LogInformation("Changing status of order {OrderId} to {Status}", id, status);

        Order? order = await _orderRepository.GetById(id);
        if (order == null)
        {
            _logger.LogWarning("Change order status failed — order {OrderId} not found", id);
            throw new NotFoundException(nameof(Order), id);
        }

        var previousStatus = order.Status;
        order.Status = status;

        bool result = await _orderRepository.Update(order);
        if (!result)
        {
            _logger.LogError("Failed to update status of order {OrderId} from {PreviousStatus} to {NewStatus}",
                id, previousStatus, status);
            return Results.BadRequest(new { message = "Failed to update order" });
        }

        _logger.LogInformation("Order {OrderId} status changed from {PreviousStatus} to {NewStatus}",
            id, previousStatus, status);

        OrderDTO dto = OrderMapper.ToDto(order);
        return Results.Ok(new { message = "The status of order was updated", order = dto });
    }

    public async Task<IResult> GetOrders(string id)
    {
        _logger.LogInformation("Fetching orders for user {UserId}", id);

        List<Order> query = await _orderRepository.GetByUser(id);
        List<OrderDTO> orders = query.Select(OrderMapper.ToDto).ToList();

        _logger.LogInformation("Fetched {Count} orders for user {UserId}", orders.Count, id);
        return Results.Ok(new { message = "The orders are fetched", orders });
    }
}