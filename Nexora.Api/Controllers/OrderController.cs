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

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IResult> GetOrders()
    {
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _orderService.GetOrders(id);

    }
    
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddOrder()
    {
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _orderService.AddOrder(id);
    }

    [Authorize]
    [HttpPatch]
    public async Task<IResult> ChangeStatus([FromQuery] Guid id, OrderStatus status)
    {
        return await _orderService.ChangeOrderStatus(id, status);
    } 
}