using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IGetProductService
{
    public Task<GetProductResponse> GetProductsService(GetProductsCommand request);

    public Task<ProductDto> GetProductById(Guid id);

}