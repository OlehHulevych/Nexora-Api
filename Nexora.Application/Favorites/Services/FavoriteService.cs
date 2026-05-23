using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Favorites.Services;

public class FavoriteService:IFavoriteService
{
    private readonly IFavoriteListRepository _favoriteListRepository;
    private readonly IBaseRepository<FavoriteItem, Guid> _favoriteItemRepository;
    private readonly IProductRepository _productRepository;

    public FavoriteService(IFavoriteListRepository favoriteListRepository, IBaseRepository<FavoriteItem, Guid> favoriteItemRepository, IProductRepository productRepository)
    {
        _favoriteItemRepository = favoriteItemRepository;
        _favoriteListRepository = favoriteListRepository;
        _productRepository = productRepository;
    }
    public async Task<IResult> AddItemToList(string userId, Guid listingId)
    {
        FavoriteList? list = await _favoriteListRepository.GetByUserId(userId);
        if (list == null) throw new NotFoundException(nameof(FavoriteList), userId);
        Listing? favoriteProduct = await _productRepository.GetById(listingId);
        if (favoriteProduct == null) throw new NotFoundException(nameof(Listing), listingId);
        FavoriteItem newFavoriteItem = new FavoriteItem
        {
            FavoriteListId = list.Id,
            FavoriteList = list,
            ListingId = listingId,
            Listing = favoriteProduct,
            
        };
        await _favoriteItemRepository.Add(newFavoriteItem);
        list.FavoriteItems.Add(newFavoriteItem);
        await _favoriteListRepository.Update(list);
        FavoriteItemDto dto = new FavoriteItemDto(newFavoriteItem.Id, favoriteProduct.Name,favoriteProduct.Price,list.Id);
        return Results.Ok(new {message="Item was added", item = dto});



    }

    public async Task<IResult> GetFavoriteList(string userId)
    {
        FavoriteList? list = await _favoriteListRepository.GetByUserId(userId);
        if (list == null) throw new NotFoundException(nameof(FavoriteList), userId);
        List<FavoriteItemDto> favoritesDto = list.FavoriteItems.Select(item => new FavoriteItemDto(item.Id,item.Listing.Name,item.Listing.Price,list.Id)).ToList();
        return Results.Ok(new {message="favorites are fetched", list = favoritesDto});
    }

    public Task<IResult> DeleteItemFromList(Guid id)
    {
        throw new NotImplementedException();
    }
}