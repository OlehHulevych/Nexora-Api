using Microsoft.Extensions.DependencyInjection;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Application.Users.Services;

namespace Nexora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAvatarService, AvatarService>();
        services.AddScoped<IListingService, IListingService>();
        return services;
    }
}