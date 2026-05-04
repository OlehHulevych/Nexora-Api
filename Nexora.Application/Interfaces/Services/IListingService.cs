using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IListingService
{
    public Task<IResult> AddProduct(CreateProductCommand data, string id);

    public Task<IResult> GetProductsService(GetProductsCommand request);

    public Task<IResult> GetProductById(Guid id);

    public Task<IResult> UpdateProductHandler(UpdateProductCommand request);
    public Task<IResult?> RemoveListing(Guid listingId, string userId);
}