namespace Nexora.Domain.DTOs;

public record CartDto(Guid Id, string BuyerName, List<CartItemDto> Items );