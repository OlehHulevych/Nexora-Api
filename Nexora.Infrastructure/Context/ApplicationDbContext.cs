using Nexora.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nexora.Application.Interfaces.Context;

namespace Nexora.Infrastructure.Context;

public class ApplicationDbContext:IdentityDbContext<ApplicationUser>,IApplicationDbContext
{
    private readonly IConfiguration config;
    public DbSet<Avatar> Avatars { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
      
    }
    
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Avatar>()
            .HasOne(a => a.User)
            .WithOne(u => u.Avatar)
            .HasForeignKey<Avatar>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasOne(u=>u.Address)
            .WithOne(a => a.User)
            .HasForeignKey<Address>(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasOne(o => o.Buyer)
            .WithMany(u => u.OrderAsBuyer)
            .HasForeignKey(o => o.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Address>()
            .HasIndex(a => a.UserId)
            .IsUnique();

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Listing>()
            .HasOne(p => p.Seller)
            .WithMany(s => s.ProductAsSeller)
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ProductImage>()
            .HasIndex(i => new { i.ProductId })
            .IsUnique(false);

        builder.Entity<Cart>()
            .HasMany(c => c.items)
            .WithOne(ci=>ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne()
            .HasForeignKey<Cart>(ct => ct.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    
}