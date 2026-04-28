using Nexora.Domain.DTOs;

namespace Nexora.Application.Product.Responses;

public record GetProductResponse (List<ProductDto> Products, int CurrentPage, int TotalPage );