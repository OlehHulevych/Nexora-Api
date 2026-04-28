using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Product.Command;
using Nexora.Infrastructure.Repository;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/listing")]
public class ProductController:ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    [HttpPost]
    [Authorize]
    [Route("/create")]
    
    public async Task<IResult> AddListing([FromForm] CreateProductCommand data)
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            return Results.Unauthorized();
        }
        return await _productRepository.CreateProduct(data, id);
    }

    [HttpGet]
    public async Task<IResult> GetListings([FromQuery] GetProductsCommand command)
    {
        return await _productRepository.GetAllProduct(command);
    }
    
    [Route("one")]
    [HttpGet]
    public async Task<IResult> GetListingById([FromQuery] Guid id)
    {
        return await _productRepository.GetProductById(id);
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IResult> Delete([FromQuery] Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Results.Unauthorized();
        }
        return await _productRepository.DeleteProduct(id, userId);
    }

    [Authorize]
    [HttpPost("update")]
    
    
    public async Task<IResult> Update([FromForm] UpdateProductCommand command)
    {
        return await _productRepository.UpdateProduct(command);
    }
    
    
    
}