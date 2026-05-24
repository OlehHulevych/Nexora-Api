using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;

namespace Nexora.Infrastructure.Repository;

public class FavoriteListRepository:IFavoriteListRepository
{
    private readonly IApplicationDbContext _context;

    public FavoriteListRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Add(FavoriteList? item)
    {
        if (item == null) throw new ArgumentException();
        await _context.FavoriteLists.AddAsync(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Update(FavoriteList item)
    {
        if (item == null) throw new ArgumentException();
         _context.FavoriteLists.Update(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<FavoriteList?> GetById(Guid id)
    {
        return await _context.FavoriteLists.Include(fl=>fl.FavoriteItems).ThenInclude(i=>i.Listing).FirstOrDefaultAsync(fl => fl.Id == id);
    }

    public async Task<bool> Delete(Guid id)
    {
        await _context.FavoriteLists.Where(fl=>fl.Id==id).ExecuteDeleteAsync();
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<FavoriteList?> GetByUserId(string id)
    {
        return await _context.FavoriteLists.Include(fl=>fl.FavoriteItems).ThenInclude(i=>i.Listing).AsNoTracking().FirstOrDefaultAsync(fl => fl.UserId == id);

    }
}