using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Product.Command;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/listing")]
public class ProductController:ControllerBase
{
    private readonly IListingService _listingService;

    public ProductController(IListingService listingService)
    {
        _listingService = listingService;
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
        return await _listingService.AddProduct(data, id);
    }

    [HttpGet]
    public async Task<IResult> GetListings([FromQuery] GetProductsCommand command)
    {
        return await _listingService.GetProductsService(command);
    }
    
    [Route("one")]
    [HttpGet]
    public async Task<IResult> GetListingById([FromQuery] Guid id)
    {
        return await _listingService.GetProductById(id);
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IResult?> Delete([FromQuery] Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Results.Unauthorized();
        }
        return await _listingService.RemoveListing(id, userId);
    }

    [Authorize]
    [HttpPost("update")]
    
    
    public async Task<IResult> Update([FromForm] UpdateProductCommand command)
    {
        return await _listingService.UpdateProductHandler(command);
    }
    
    
    
}