using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Domain.DTOs;
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

    public async Task<LoginResponse> LoginUserHandler(LoginUserCommand request)
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

    public async Task<UserDto> RetrieveUser(string Id)
    {
        if (Id.IsNullOrEmpty())
        {
            throw new BadHttpRequestException("There is no user`id");
        }

        var user = await _userManager.Users.Include(u=>u.Avatar).Include(u=>u.Address).FirstOrDefaultAsync(u=>u.Id.Equals(Id));
        if (user == null)
        {
            throw new UserIsNotFoundException();
        }

        return new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email, user.Avatar.Uri,
            user.Address?.Line1 + ", "+ user.Address?.City + ", "+user.Address?.Country);

    }

}