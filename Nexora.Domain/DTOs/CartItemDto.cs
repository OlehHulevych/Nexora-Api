namespace Nexora.Domain.DTOs;

public record CartItemDto(Guid Id, string Name, decimal Price, int Quantity, decimal Subtotal);
