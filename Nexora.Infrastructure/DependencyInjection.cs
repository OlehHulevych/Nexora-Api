using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Infrastructure.Context;
using Nexora.Infrastructure.JWT;
using Nexora.Infrastructure.Repository;
using Nexora.Infrastructure.Storage;

namespace Nexora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserBlobStorage, UserBlobStorageService>();
        services.AddScoped<IProductBlobStorage, ProductBlobStorageService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAvatarRepository, AvatarRepository>();
        services.AddScoped<IJwtService, JwtTokenHandler>();
        services.AddScoped<IListingPhotoRepository, ListingPhotoRepository>();
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.Section));

        return services;
    }
}