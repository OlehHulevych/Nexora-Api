using Microsoft.AspNetCore.Http;
using Nexora.Application.Product.Command;
using Nexora.Application.Product.Responses;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Repositories;

public interface IProductRepository
{
    public Task<Guid?> Create(Listing listing);
    public Task<IQueryable<Listing>?> GetAll(GetProductsCommand request);
    public Task<Listing?> GetById(Guid? id);
    public Task? Update(Listing listing);
    public Task<IResult?> Delete(Guid listingId, string userId);
}