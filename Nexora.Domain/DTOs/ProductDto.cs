namespace Nexora.Domain.DTOs;

public record ProductDto(
    string Name,
    string Description,
    decimal Price,
    int? StockQuantity,
    bool isActive,
    String SellerId,
    string? Category,
    IEnumerable<string> Photos,
    IEnumerable<ReviewDto> Reviews );
    