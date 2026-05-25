using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.Entities;

namespace Nexora.Api.Controllers;

[ApiController]
[Route("api/order")]
public class OrderController : Controller
{
    // GET
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }
    
    /// <summary>
    /// Getting orders of user
    /// </summary>
    /// <returns>orders</returns>
    /// <response code="200">orders are fetched</response>
    /// <response code="404">orders are not existing</response>
    [Authorize]
    [HttpGet]
    public async Task<IResult> GetOrders()
    {
        _logger.LogInformation("Order controller - getting orders");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _orderService.GetOrders(id);

    }
    
    /// <summary>
    /// Creating new order
    /// </summary>
    /// <returns>created order</returns>
    /// <response code="200">order is created</response>
    /// <response code="404">failed to create order, cart is empty</response>
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddOrder()
    {
        _logger.LogInformation("Order controller - creating order");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _orderService.AddOrder(id);
    }
    
    /// <summary>
    /// Changing status of order
    /// </summary>
    /// <param name="id">Order id</param>
    /// <param name = "status">Order status</param>
    /// <returns>order with changed status</returns>
    /// <response code="200">order status was changed</response>
    /// <response code="404">order is not existing</response>
    [Authorize]
    [HttpPatch]
    public async Task<IResult> ChangeStatus([FromQuery] Guid id, OrderStatus status)
    {
        _logger.LogInformation("Order controller - changing status of order");
        return await _orderService.ChangeOrderStatus(id, status);
    } 
}