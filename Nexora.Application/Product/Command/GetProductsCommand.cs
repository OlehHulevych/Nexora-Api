namespace Nexora.Application.Product.Command;
using Microsoft.AspNetCore.Http;

public record GetProductsCommand(
    string? Name,
    string? category,
    int page = 1
    );