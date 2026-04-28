namespace Nexora.Application.Product.Command;
using Microsoft.AspNetCore.Http;

public  record UpdateProductCommand(
    Guid listingId,
    string? Name,
    string? Description,
    decimal?  Price,
    int? StockQuantity,
    string? Category,
    List<IFormFile>? Photos,
    List<string>? PhotosForDelete
    );