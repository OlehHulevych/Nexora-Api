using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;

namespace Nexora.Application.Interfaces.Context;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<IdentityRole> Roles { get; }
    DbSet<ProductImage> ProductImages { get; }
    public DbSet<Domain.Entities.Review> Reviews { get; set; }
    DbSet<Domain.Entities.Cart> Carts { get;}
    DbSet<CartItem> CartItems { get;}
    DbSet<IdentityUserRole<string>> UserRoles { get; }
    DbSet<Domain.Entities.Category> Categories { get; }
    DbSet<IdentityUserClaim<string>> UserClaims { get; }
    DbSet<IdentityUserLogin<string>> UserLogins { get; }
    DbSet<IdentityUserToken<string>> UserTokens { get; }
    DbSet<IdentityRoleClaim<string>> RoleClaims { get; }
    DbSet<Listing> Listings { get; set; }
    DbSet<Domain.Entities.Address> Addresses { get; }
    DbSet<Avatar> Avatars { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
}