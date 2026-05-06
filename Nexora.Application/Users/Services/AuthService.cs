using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Application.Interfaces.Repositories;
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
    private readonly IAvatarService _avatarService;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    
    private ILogger<AuthService> _logger;

    public AuthService( IAvatarService avatarService, ILogger<AuthService> logger, 
        IJwtService jwtService,IUserRepository userRepository)
    {
        _avatarService = avatarService;
        _logger = logger;
        _jwtService = jwtService;
      
        _userRepository = userRepository;
    }
    
    public async Task<IResult> RegisterUserService(RegisterUserCommand request)
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
            
            var result = await _userRepository.AddUser(user, request.Password,null,null);
            if (!result) throw new BadHttpRequestException("Failed to create new user");
            Domain.Entities.Cart newCart = new Domain.Entities.Cart()
            {
                UserId = user.Id
            };
            Domain.Entities.Address address = new Domain.Entities.Address(user.Id, request.Address.Line1,
                request.Address.City, request.Address.Country, request.Address.PostalCode, request.Address.Line2);
            user.Cart = newCart;
            var resultRoles = await _userRepository.AddRole(user, RoleNames.User);
            if (!resultRoles) throw new Exception("Failed to load role to user");
            UploadAvatarResponse avatarResponse =
                await _avatarService.UploadAvatar(new UploadAvatarCommand(user.Id, user, request.Avatar), request.FirstName+"_"+request.LastName);
            if (avatarResponse.uri.IsNullOrEmpty()) throw new Exception("Register failed during the uploading avatar");
            user.Avatar = avatarResponse.Avatar;
            user.Address = address;
            await _userRepository.UpdateUser(user);
            RegisterUserResponse registerUserResult = new RegisterUserResponse(user.Id, user.Email, user.FirstName, user.LastName, address.Line1);
            _logger.LogInformation("The user is registered");
            return Results.Ok(new {message = "The user was registered", data = registerUserResult});

    }
    
    public async Task<IResult> LoginUserHandler(LoginUserCommand? request)
    {
        if (request != null && (request.email.IsNullOrEmpty() || request.password.IsNullOrEmpty()))
        {
            throw new ValidationException("Required data is missing");
        }

        if (request != null)
        {
            var user = await _userRepository.FindByEmail(request.email);
            if (user == null)
            {
                throw new UserIsNotFoundException();
            }

            var result = await _userRepository.CheckPassword(user, request.password);
            if (!result)
            {
                throw new PasswordIsNotMatched();
            }

            string? token = await _jwtService.CreateToken(user);
            if (token.IsNullOrEmpty())
            {
                throw new BadHttpRequestException("Failed during creating token");
            }

            if (token != null) return Results.Ok(new { message = "The user was logged", data = new LoginResponse(token) });
        }

        return Results.BadRequest(new {messaage = "failed to register user"});
    }

    public async Task<IResult> RetrieveUser(string id)
    {
        if (id.IsNullOrEmpty())
        {
            throw new BadHttpRequestException("There is no user`id");
        }

        var user = await _userRepository.FindByEmail(id);
        if (user == null)
        {
            throw new UserIsNotFoundException();
        }

        if (user.Email != null)
            return Results.Ok(new
            {
                message = "User is fetched", data = new UserDto(user.Id, user.FirstName + " " + user.LastName,
                    user.Email, user.Avatar?.Uri,
                    user.Address?.Line1 + ", " + user.Address?.City + ", " + user.Address?.Country)
            });
        return Results.BadRequest(new {message = "Failed to retrieve user"});
    }

    private async Task<bool> CheckIfuserExistByEmail(string email)
    {
        var result = await _userRepository.FindByEmail(email);
        return result == null && false;
    }

    private bool MatchingPasswordHandler(string password, string confirmPassword)
    {
        return password.Equals(confirmPassword);
    }
}