using Microsoft.Extensions.DependencyInjection;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Services;

namespace Nexora.Application;

public static class DependectyInjection
{
    public static IServiceCollection AddApplicatoin(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}