using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;

namespace Nexora.Domain.Mappers;

public static class OrderMapper
{
    public static OrderDTO ToDto(Order? order)
    {
        if (order == null || order.DeliveredAddress==null|| order.DeliveredAddress.PostalCode==null || order.Items?.Count>0) throw new ArgumentException();
        return new OrderDTO(
            order.Id,
            order.Status.ToString(),
            order.CreatedAt,
            order.CreatedAt,
            order.TotalAmount,
            new AddressDto(order.DeliveredAddress.Line1, order.DeliveredAddress.Line2, order.DeliveredAddress.City,
                order.DeliveredAddress.Country, order.DeliveredAddress.PostalCode),
            order.Items?
                .Where(item => item.Product != null)
                .Select(item => new OrderItemDto(
                    item.Id,
                    item.Product!.Name,
                    item.Product.Images.FirstOrDefault()?.Url,
                    item.Product.Price,
                    item.Quantity,
                    item.Quantity * item.Product.Price
                ))
                .ToList()
        );
    }
}