using Microsoft.AspNetCore.Identity;

using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Config;
using Nexora.Domain.Constants;

namespace Nexora.Application.Users.Services;

public class GoogleAuthService:IGoogleAuthService
{
    
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGoogleConfig _googleConfig;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService( UserManager<ApplicationUser> userManager, IGoogleConfig googleConfig, ILogger<GoogleAuthService> logger)
    {
        _userManager = userManager;
        _googleConfig = googleConfig;
        _logger = logger;
    }
    public async Task<UserDto?> GoogleSignIn(GoogleSignInVM? model)
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
            UserName = payload.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors )
            {
                _logger.LogInformation(error.Code);
                _logger.LogInformation(error.Description);
            }
            throw new BadHttpRequestException("Something went wrong during authentication");
        }

        await _userManager.AddLoginAsync(user, new UserLoginInfo(
            LoginNames.Google,
            payload.Subject,
            LoginNames.Google));
        return new UserDto(user.Id, user.FirstName+ " "+user.LastName,user.Email);
    }
}