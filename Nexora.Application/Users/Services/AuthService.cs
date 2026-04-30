using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Nexora.Application.Users.Services;

public class AuthService:IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IAvatarService _avatarService;
    private readonly IJwtService _jwtService;
    private ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, IApplicationDbContext context, IAvatarService avatarService, ILogger<AuthService> logger, IJwtService jwtService)
    {
        _context = context;
        _userManager = userManager;
        _avatarService = avatarService;
        _logger = logger;
        _jwtService = jwtService;
    }
    
    public async Task<RegisterUserResponse> RegisterUserService(RegisterUserCommand request)
    {
            bool userExist = await CheckIfuserExistByEmail(request.Email);
            if (userExist)
            {
                throw new UserAlreadyExistsException(request.Email);
            }
            var matchPassword = MatchingPasswordHandler(request.Password, request.ConfirmPassword);
            if (!matchPassword)
            {
                throw new PasswordIsNotMatched();
            }
            ApplicationUser user = new ApplicationUser()
            {
                UserName = request.FirstName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,


            };
            
            var result = await _userManager.CreateAsync(user, request.Password);
            Domain.Entities.Cart newCart = new Domain.Entities.Cart()
            {
                UserId = user.Id
            };
            Domain.Entities.Address address = new Domain.Entities.Address(user.Id, request.Address.Line1,
                request.Address.City, request.Address.Country, request.Address.PostalCode, request.Address.Line2);
            await _context.Carts.AddAsync(newCart);
            await _context.Addresses.AddAsync(address);
            user.Cart = newCart;
            var resultRoles = await _userManager.AddToRoleAsync(user, RoleNames.User);
            if (!resultRoles.Succeeded)
            {
                throw new Exception("Failed to load role to user");
            }
            UploadAvatarResponse avatarResponse =
                await _avatarService.UploadAvatar(new UploadAvatarCommand(user.Id, user, request.Avatar), request.FirstName+"_"+request.LastName);
            if (avatarResponse.uri.IsNullOrEmpty())
            {
                throw new Exception("Register failed during the uploading avatar");
            }
            user.Avatar = avatarResponse.Avatar;
            user.Address = address;
            await _context.SaveChangesAsync();
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new AuthenticationFailureException(errors); 
            }
            RegisterUserResponse registerUserResult = new RegisterUserResponse(user.Id, user.Email, user.FirstName, user.LastName, address.Line1);
            _logger.LogInformation("The user is registered");
            return registerUserResult;

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

    public async Task<UserDto> RetrieveUser(string id)
    {
        if (id.IsNullOrEmpty())
        {
            throw new BadHttpRequestException("There is no user`id");
        }

        var user = await _userManager.Users.Include(u=>u.Avatar).Include(u=>u.Address).FirstOrDefaultAsync(u=>u.Id.Equals(id));
        if (user == null)
        {
            throw new UserIsNotFoundException();
        }

        return new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email, user.Avatar.Uri,
            user.Address?.Line1 + ", "+ user.Address?.City + ", "+user.Address?.Country);

    }

    private async Task<bool> CheckIfuserExistByEmail(string email)
    {
        var result = await _userManager.FindByEmailAsync(email);
        return result == null && false;
    }

    private bool MatchingPasswordHandler(string password, string confirmPassword)
    {
        return password.Equals(confirmPassword);
    }
}