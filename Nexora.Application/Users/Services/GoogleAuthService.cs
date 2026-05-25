using System.Security.Authentication;
using Nexora.Application.Interfaces.Services;
using Nexora.Domain.DTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Config;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Enums;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Users.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IHttpContextAccessor _context;
    private readonly IGoogleConfig _googleConfig;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly IUserRepository _userRepository;

    public GoogleAuthService(IUserRepository userRepository, IGoogleConfig googleConfig, ILogger<GoogleAuthService> logger, IJwtService jwtService, IHttpContextAccessor context)
    {
        _googleConfig = googleConfig;
        _logger = logger;
        _jwtService = jwtService;
        _context = context;
        _userRepository = userRepository;
    }

    public async Task<IResult> GoogleSignIn(GoogleSignInVM? model)
    {
        _logger.LogInformation("Google sign-in attempt started");

        if (model?.IdToken == null)
        {
            _logger.LogWarning("Google sign-in failed — IdToken is missing");
            throw new BadHttpRequestException("IdToken is required");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            _logger.LogInformation("Validating Google IdToken");
            payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleConfig.ClientId }
                });
            _logger.LogInformation("Google IdToken validated successfully for email {Email}", payload.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google IdToken validation failed");
            throw;
        }

        var user = new ApplicationUser()
        {
            FirstName = payload.GivenName,
            LastName = payload.FamilyName,
            Email = payload.Email,
            UserName = payload.Email,
            EmailConfirmed = true
        };

        _logger.LogInformation("Creating account for Google user {Email}", payload.Email);
        var createResult = await _userRepository.AddUser(user, null, LoginProvider.Google, payload);
        if (!createResult)
        {
            _logger.LogError("Failed to create account for Google user {Email}", payload.Email);
            throw new BadHttpRequestException("Something went wrong during authentication");
        }

        _logger.LogInformation("Google user {Email} registered successfully with Id {UserId}", user.Email, user.Id);
        return Results.Ok(new
        {
            message = "The user logged in",
            data = new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email)
        });
    }

    public async Task<IResult> GoogleLogIn(GoogleSignInVM? model)
    {
        _logger.LogInformation("Google login attempt started");

        if (model?.IdToken == null)
        {
            _logger.LogWarning("Google login failed — IdToken is missing");
            throw new BadHttpRequestException("IdToken is required");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            _logger.LogInformation("Validating Google IdToken");
            payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleConfig.ClientId }
            });
            _logger.LogInformation("Google IdToken validated for email {Email}", payload.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google IdToken validation failed during login");
            throw;
        }

        _logger.LogInformation("Looking up user by email {Email}", payload.Email);
        var user = await _userRepository.FindByEmail(payload.Email);
        if (user == null)
        {
            _logger.LogWarning("Google login failed — user {Email} not found", payload.Email);
            throw new NotFoundException(nameof(ApplicationUser), payload.Email);
        }

        _logger.LogInformation("Generating JWT token for user {Email}", payload.Email);
        var token = await _jwtService.CreateToken(user);
        if (token == null)
        {
            _logger.LogError("Failed to generate JWT token for user {Email}", payload.Email);
            throw new AuthenticationException("Failed to create token");
        }

        var cookieOptions = new CookieOptions()
        {
            IsEssential = true,
            Expires = DateTime.Now.AddDays(7),
            Secure = true,
            HttpOnly = false,
            SameSite = SameSiteMode.None
        };

        _context.HttpContext?.Response.Cookies.Append("token", token, cookieOptions);
        _logger.LogInformation("Google user {Email} logged in successfully", payload.Email);

        return Results.Ok(new { message = "The user is logged in" });
    }
}