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

    public FavoriteListController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IResult> GetList()
    {
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _favoriteService.GetFavoriteList(id);
    }

    [Authorize]
    [HttpPost]
    public async Task<IResult> AddItemToList([FromBody] Guid listingId)
    {
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) throw new UnauthorizedAccessException();
        return await _favoriteService.AddItemToList(id,listingId);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IResult> RemoveItemFromList([FromBody] Guid id)
    {
        return await _favoriteService.DeleteItemFromList(id);
    }
}