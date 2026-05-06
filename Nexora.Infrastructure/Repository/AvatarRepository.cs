using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class AvatarRepository:IAvatarRepository
{
    private readonly IApplicationDbContext _context;

    public AvatarRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Add(Avatar avatar)
    {
        await _context.Avatars.AddAsync(avatar);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> Update(Avatar avatar)
    {
         _context.Avatars.Update(avatar);
         var result = await _context.SaveChangesAsync();
         return result > 0;
    }

    public async Task<Avatar?> GetById(Guid id)
    {
        Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(a=>a.Id==id);
        if (avatar == null) throw new NotFoundException(nameof(Avatar), id);
        return avatar;
    }

    public async Task<Avatar?> GetByUserId(string id)
    {
        Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(a=>a.UserId==id);
        if (avatar == null) throw new NotFoundException(nameof(Avatar), id);
        return avatar;    }

    public async Task<bool> Delete(Guid id)
    {
        Avatar? avatar = await GetById(id);
        if (avatar == null) throw new NotFoundException(nameof(Avatar), id);
        _context.Avatars.Remove(avatar);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}