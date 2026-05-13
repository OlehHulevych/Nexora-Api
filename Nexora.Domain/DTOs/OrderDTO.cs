namespace Nexora.Domain.DTOs;

public record OrderDTO(Guid Id, string Status,DateTime CreatedAt, DateTime UpdatedAt, decimal TotalPrice,AddressDto Address, List<OrderItemDto>? Items);