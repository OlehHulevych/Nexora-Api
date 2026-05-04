using Microsoft.AspNetCore.Http;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Repositories;

public interface IProductRepository
{
    public Task<Guid?> CreateProduct(Listing listing);
    public Task<GetProductResponse?> GetAllProduct(GetProductsCommand request);
    public Task<Listing?> GetProductById(Guid id);
    public Task? UpdateProduct(Listing listing);
    public Task<IResult?> DeleteProduct(Guid listingId, string userId);
}