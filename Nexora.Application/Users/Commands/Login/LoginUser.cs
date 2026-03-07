using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace Nexora.Application.Users.Commands.Login;

public class LoginUser
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public LoginUser(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> loginUserHandler(LoginUserCommand request)
    {
        if (request.email.IsNullOrEmpty() || request.password.IsNullOrEmpty())
        {
            throw new ValidationException("Required data is missing");
        }

        var user = await _userManager.FindByEmailAsync(request.email);
        if (user == null)
        {
            throw new UserIsNotFoundException();
        }

        var result = await _userManager.CheckPasswordAsync(user, request.password);
        if (!result)
        {
            throw new PasswordIsNotMatched();
        }

        var token = await _jwtService.CreateToken(user);
        if (token.IsNullOrEmpty())
        {
            throw new BadHttpRequestException("Failed during creating token");
        }

        return new LoginResponse(token);
    }

}