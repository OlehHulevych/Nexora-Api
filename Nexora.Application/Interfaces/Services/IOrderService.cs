using Microsoft.AspNetCore.Http;

namespace Nexora.Application.Interfaces.Services;

public interface IOrderService
{
    public Task<IResult> AddOrder(string id);
    public Task<IResult> ChangeOrderStatus(Guid id, OrderStatus status);
}