using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class FavoriteItemRepository:IBaseRepository<FavoriteItem, Guid>
{
    private readonly IApplicationDbContext _context;

    public FavoriteItemRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Add(FavoriteItem? item)
    {
        if (item == null) throw new ArgumentException();
        await _context.FavoriteItems.AddAsync(item);
        return await _context.SaveChangesAsync() > 0;

    }

    public async Task<bool> Update(FavoriteItem item)
    {
        if (item == null) throw new ArgumentException();
         _context.FavoriteItems.Update(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<FavoriteItem?> GetById(Guid id)
    {
        return await _context.FavoriteItems.FirstOrDefaultAsync(i=>i.Id==id);
    }

    public async Task<bool> Delete(Guid id)
    {
        FavoriteItem? item = await GetById(id);
        if (item == null) throw new NotFoundException(nameof(FavoriteItem), id);
        return await _context.SaveChangesAsync() > 0;
    }
}