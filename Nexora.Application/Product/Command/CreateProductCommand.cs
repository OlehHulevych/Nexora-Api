namespace Nexora.Application.Product.Command;
using Microsoft.AspNetCore.Http;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int? StockQuantity,
    string Category,
    List<IFormFile> Photos
    );