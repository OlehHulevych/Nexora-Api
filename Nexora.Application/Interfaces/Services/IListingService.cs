using Microsoft.EntityFrameworkCore;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IListingService
{
    public Task<Listing> AddProduct(CreateProductCommand data, string id);

    public Task<GetProductResponse> GetProductsService(GetProductsCommand request);

    public Task<ProductDto> GetProductById(Guid id);

    public Task<Guid> UpdateProductHandler(UpdateProductCommand request);
}