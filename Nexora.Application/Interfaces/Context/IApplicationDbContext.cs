
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Nexora.Application.Interfaces.Context;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Order> Orders { get; }
    DbSet<IdentityRole> Roles { get; }
    DbSet<ProductImage> ProductImages { get; }
    public DbSet<Review> Reviews { get; set; }
    DbSet<Cart> Carts { get;}
    DbSet<FavoriteList> FavoriteLists { get; set; }
    DbSet<FavoriteItem> FavoriteItems { get; set; }
    DbSet<ReviewLike> ReviewLikes { get; }
    DbSet<CartItem> CartItems { get;}
    DbSet<IdentityUserRole<string>> UserRoles { get; }
    DbSet<Domain.Entities.Category> Categories { get; }
    DbSet<IdentityUserClaim<string>> UserClaims { get; }
    DbSet<IdentityUserLogin<string>> UserLogins { get; }
    DbSet<IdentityUserToken<string>> UserTokens { get; }
    DbSet<IdentityRoleClaim<string>> RoleClaims { get; }
    DbSet<Listing> Listings { get; set; }
    DbSet<Domain.Entities.Address> Addresses { get; }
    DbSet<T> Set<T>() where T : class;
    DbSet<Avatar> Avatars { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
}