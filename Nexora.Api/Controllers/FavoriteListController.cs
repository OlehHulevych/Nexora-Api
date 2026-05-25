using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Services;

namespace Nexora.Api.Controllers;

[ApiController]
[Route("api/favorite")]
public class FavoriteListController:Controller
{
    private readonly IFavoriteService _favoriteService;
    private readonly ILogger<FavoriteListController> _logger;
    

    public FavoriteListController(IFavoriteService favoriteService, ILogger<FavoriteListController> logger)
    {
        _favoriteService = favoriteService;
        _logger = logger;
        
    }
    
    /// <summary>
    /// Fetching favorite items
    /// </summary>
    /// <returns>favorite list</returns>
    /// <response code="200">favorite list is fetched</response>
    /// <response code="404">favorite list does not exist</response>
    [Authorize]
    [HttpGet]
    public async Task<IResult> GetList()
    {
        _logger.LogInformation("FavoriteList Controller - getting favorite list");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _favoriteService.GetFavoriteList(id);
    }
    
    /// <summary>
    /// Adding item to favorite list
    /// </summary>
    /// <param name="listingId">contain listing id</param>
    /// <returns>Added favorite item</returns>
    /// <response code="200">Item added successfully to favorite list</response>
    /// <response code="404">Failed to add item to favorite list</response>
    [Authorize]
    [HttpPost]
    public async Task<IResult> AddItemToList([FromBody] Guid listingId)
    {
        _logger.LogInformation("FavoriteList Controller - adding item to list");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _favoriteService.AddItemToList(id,listingId);
    }
    /// <summary>
    /// Removing item from favorite list
    /// </summary>
    /// <param name="id">id of item id</param>
    /// <returns>Added favorite item</returns>
    /// <response code="200">Item remove successfully</response>
    /// <response code="404">Failed to remove item from favorite list</response>
    [Authorize]
    [HttpDelete]
    public async Task<IResult> RemoveItemFromList([FromBody] Guid id)
    {
        _logger.LogInformation("FavoriteList Controller - removing item from list");
        return await _favoriteService.DeleteItemFromList(id);
    }
}