using Microsoft.EntityFrameworkCore;
using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.Context;

public interface IApplicationDbContext
{
    DbSet<Domain.Entities.Address> Addresses { get; }
    DbSet<Avatar> Avatars { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
}