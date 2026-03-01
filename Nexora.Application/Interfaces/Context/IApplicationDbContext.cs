using Microsoft.EntityFrameworkCore;

namespace Nexora.Application.Interfaces.Context;

public interface IApplicationDbContext
{
    DbSet<Domain.Entities.Address> Addresses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
}