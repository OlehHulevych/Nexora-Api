namespace Nexora.Domain.DTOs;

public record OrderItemDto(Guid Id, string ProductName, string? ImageUrl, decimal Price, int Quantity, decimal SubTotal);