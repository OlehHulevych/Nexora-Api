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
    private readonly ILogger<ProductController> _logger;

    public ProductController(IListingService listingService, ILogger<ProductController> logger)
    {
        _listingService = listingService;
        _logger = logger;
    }
    /// <summary>
    /// Creating listing
    /// </summary>
    /// <param name="data">all needed data for creating listing</param>
    /// <returns>created listing</returns>
    /// <response code="200">listing was created successfully</response>
    [HttpPost]
    [Authorize]
    [Route("/create")]
    
    public async Task<IResult> AddListing([FromForm] CreateProductCommand data)
    {
        _logger.LogInformation("Listing controller - creating new listing");
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            _logger.LogError("Listing controller - user is unauthorized");
            return Results.Unauthorized();
        }
        return await _listingService.AddProduct(data, id);
    }
    /// <summary>
    /// Getting listings
    /// </summary>
    /// <param name="command">request object contains information about pagination</param>
    /// <returns>listings</returns>
    /// <response code="200">listings were fetched successfully</response>
    [HttpGet]
    public async Task<IResult> GetListings([FromQuery] GetProductsCommand command)
    {
        _logger.LogInformation("Listing controller - getting listings by pagination");
        return await _listingService.GetProductsService(command);
    }
    /// <summary>
    /// Getting listing by id
    /// </summary>
    /// <param name="id">listing id</param>
    /// <returns>listing</returns>
    /// <response code="200">listing was fetched successfully</response>
    /// <response code="404">listing is not existing</response>
    [Route("one")]
    [HttpGet]
    public async Task<IResult> GetListingById([FromQuery] Guid id)
    {
        _logger.LogInformation("Listing controller - getting listing by id");
        return await _listingService.GetProductById(id);
    }
    /// <summary>
    /// Deleting listing by id
    /// </summary>
    /// <param name="id">listing id</param>
    /// <returns>listing</returns>
    /// <response code="200">listing was deleted successfully</response>
    /// <response code="404">listing is not existing</response>
    [Authorize]
    [HttpDelete]
    public async Task<IResult?> Delete([FromQuery] Guid id)
    {
        _logger.LogInformation("Listing controller - deleting listing by id");
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogError("Listing controller - user is unauthorized");
            return Results.Unauthorized();
        }
        return await _listingService.RemoveListing(id, userId);
    }
    /// <summary>
    /// Getting listing by id
    /// </summary>
    /// <param name="command">Command contains listing id, and new data for existing listing id to update</param>
    /// <returns>listing</returns>
    /// <response code="200">listing was updated successfully</response>
    /// <response code="404">listing is not existing</response>
    [Authorize]
    [HttpPost("update")]
    public async Task<IResult> Update([FromForm] UpdateProductCommand command)
    {
        return await _listingService.UpdateProductHandler(command);
    }
    
    
    
}