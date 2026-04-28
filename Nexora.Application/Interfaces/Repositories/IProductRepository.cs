using Microsoft.AspNetCore.Http;
using Nexora.Application.Product.Command;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Repositories;

public interface IProductRepository
{
    public Task<IResult> CreateProduct(CreateProductCommand request, string id);
    public Task<IResult> GetAllProduct(GetProductsCommand request);
    public Task<IResult> GetProductById(Guid id);
    public Task<IResult> UpdateProduct(UpdateProductCommand request);
    public Task<IResult> DeleteProduct(Guid listingId, string userId);
}