using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;

using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Config;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Domain.Constants;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Users.Services;

public class GoogleAuthService:IGoogleAuthService
{
    private readonly IHttpContextAccessor _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGoogleConfig _googleConfig;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService( UserManager<ApplicationUser> userManager, IGoogleConfig googleConfig, ILogger<GoogleAuthService> logger, IJwtService jwtService, IHttpContextAccessor context)
    {
        _userManager = userManager;
        _googleConfig = googleConfig;
        _logger = logger;
        _jwtService = jwtService;
        _context = context;
    }
    public async Task<IResult> GoogleSignIn(GoogleSignInVM? model)
    {
        GoogleJsonWebSignature.Payload payload;
        payload = await GoogleJsonWebSignature.ValidateAsync(model?.IdToken,
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
        return Results.Ok(new
        {
            message = "The user logged in",
            data = new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email)
        });
    }

    public async Task<IResult> GoogleLogIn(GoogleSignInVM? model)
    {
        GoogleJsonWebSignature.Payload payload;
        payload = await GoogleJsonWebSignature.ValidateAsync(model?.IdToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new [] {_googleConfig.ClientId}
        });
        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user == null) throw new NotFoundException(nameof(ApplicationUser), payload.Email);
        var token = await _jwtService.CreateToken(user);
        if (token == null) throw new AuthenticationException("Failed to create token");
        var cookieOptions = new CookieOptions()
        {
            IsEssential = true,
            Expires = DateTime.Now.AddDays(7),
            Secure = true,
            HttpOnly = false,
            SameSite = SameSiteMode.None
        };
        _context.HttpContext?.Response.Cookies.Append("token",token, cookieOptions);
        return Results.Ok(new {message = "The user is logged in"});
    }
}