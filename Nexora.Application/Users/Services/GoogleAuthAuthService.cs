using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Config;
using Nexora.Domain.Constants;

namespace Nexora.Application.Users.Services;

public class GoogleAuthAuthService:IGoogleAuthService
{
    
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGoogleConfig _googleConfig;

    public GoogleAuthAuthService(IApplicationDbContext context, UserManager<ApplicationUser> userManager, IOptions<IGoogleConfig> googleConfig)
    {
        _userManager = userManager;
        _googleConfig = googleConfig.Value;
    }
    public async Task<UserDto> GoogleSignIn(GoogleSignInVM model)
    {
        GoogleJsonWebSignature.Payload payload = new();
        payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleConfig.ClientId }

            });
        var user = new ApplicationUser()
        {
            FirstName = payload.GivenName,
            LastName = payload.FamilyName,
            Email = payload.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            throw new BadHttpRequestException("Something went wrong during authentication");
        }

        await _userManager.AddLoginAsync(user, new UserLoginInfo(
            LoginNames.Google,
            payload.Subject,
            LoginNames.Google));
        return new UserDto(user.Id, user.FirstName+ " "+user.LastName,user.Email);
    }
}