using Microsoft.Extensions.DependencyInjection;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Product.Services;
using Nexora.Application.Review.Services;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Application.Users.Commands.Validation;
using Nexora.Application.Users.Services;

namespace Nexora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAvatarService, AvatarService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ValidationErrors>();
        
        
        return services;
    }
}