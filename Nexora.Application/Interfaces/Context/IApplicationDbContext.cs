using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;

namespace Nexora.Application.Interfaces.Context;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<IdentityRole> Roles { get; }
    DbSet<IdentityUserRole<string>> UserRoles { get; }
    DbSet<IdentityUserClaim<string>> UserClaims { get; }
    DbSet<IdentityUserLogin<string>> UserLogins { get; }
    DbSet<IdentityUserToken<string>> UserTokens { get; }
    DbSet<IdentityRoleClaim<string>> RoleClaims { get; }
    DbSet<Domain.Entities.Address> Addresses { get; }
    DbSet<Avatar> Avatars { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
}