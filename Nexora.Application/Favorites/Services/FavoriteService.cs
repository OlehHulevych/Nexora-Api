using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Favorites.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteListRepository _favoriteListRepository;
    private readonly IBaseRepository<FavoriteItem, Guid> _favoriteItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(IFavoriteListRepository favoriteListRepository,
        IBaseRepository<FavoriteItem, Guid> favoriteItemRepository,
        IProductRepository productRepository,
        ILogger<FavoriteService> logger)
    {
        _favoriteItemRepository = favoriteItemRepository;
        _favoriteListRepository = favoriteListRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<IResult> AddItemToList(string userId, Guid listingId)
    {
        _logger.LogInformation("Adding listing {ListingId} to favorites for user {UserId}", listingId, userId);

        FavoriteList? list = await _favoriteListRepository.GetByUserId(userId);
        if (list == null)
        {
            _logger.LogWarning("Add to favorites failed — favorite list not found for user {UserId}", userId);
            throw new NotFoundException(nameof(FavoriteList), userId);
        }

        Listing? favoriteProduct = await _productRepository.GetById(listingId);
        if (favoriteProduct == null)
        {
            _logger.LogWarning("Add to favorites failed — listing {ListingId} not found", listingId);
            throw new NotFoundException(nameof(Listing), listingId);
        }

        bool alreadyExists = list.FavoriteItems.Any(i => i.ListingId == listingId);
        if (alreadyExists)
        {
            _logger.LogWarning("Add to favorites failed — listing {ListingId} already in favorites for user {UserId}", listingId, userId);
            throw new ConflictException(nameof(FavoriteItem));
        }

        FavoriteItem newFavoriteItem = new FavoriteItem
        {
            FavoriteListId = list.Id,
            ListingId = listingId
        };

        await _favoriteItemRepository.Add(newFavoriteItem);
        _logger.LogInformation("Listing {ListingId} added to favorites for user {UserId}", listingId, userId);

        FavoriteItemDto dto = new FavoriteItemDto(newFavoriteItem.Id, favoriteProduct.Name, favoriteProduct.Price, list.Id);
        return Results.Ok(new { message = "Item was added", item = dto });
    }

    public async Task<IResult> GetFavoriteList(string userId)
    {
        _logger.LogInformation("Fetching favorite list for user {UserId}", userId);

        FavoriteList? list = await _favoriteListRepository.GetByUserId(userId);
        if (list == null)
        {
            _logger.LogWarning("Get favorites failed — favorite list not found for user {UserId}", userId);
            throw new NotFoundException(nameof(FavoriteList), userId);
        }

        List<FavoriteItemDto> favoritesDto = list.FavoriteItems
            .Select(item => new FavoriteItemDto(item.Id, item.Listing.Name, item.Listing.Price, list.Id))
            .ToList();

        _logger.LogInformation("Fetched {Count} favorite items for user {UserId}", favoritesDto.Count, userId);
        return Results.Ok(new { message = "favorites are fetched", list = favoritesDto });
    }

    public async Task<IResult> DeleteItemFromList(Guid id)
    {
        _logger.LogInformation("Removing favorite item {ItemId}", id);

        var result = await _favoriteItemRepository.Delete(id);
        if (!result)
        {
            _logger.LogError("Failed to remove favorite item {ItemId}", id);
            return Results.BadRequest(new { message = "Failed to remove item from list", status = result });
        }

        _logger.LogInformation("Favorite item {ItemId} removed successfully", id);
        return Results.Ok(new { message = "Item was removed", status = result });
    }
}