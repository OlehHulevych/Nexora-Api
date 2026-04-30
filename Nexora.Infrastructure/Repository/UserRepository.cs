using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.Login;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Update;
using Nexora.Application.Users.Commands.Validation;
using Nexora.Application.Users.Services;
using Nexora.Domain.DTOs;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Nexora.Infrastructure.Repository;

public class UserRepository:IUserRepository
{
    private readonly RegisterUser _registerUserHandler;
    private readonly LoginUser _loginUserHandler;
    private readonly IHttpContextAccessor _context;
    private readonly ValidationErrors _validationErrorHandler;
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly IValidator<LoginUserCommand> _loginValidator;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserService _userService;

    

    public UserRepository(IHttpContextAccessor context, RegisterUser registerUser, LoginUser loginUserHandler, IValidator<RegisterUserCommand> validator, ValidationErrors validationErrorHandler, 
        IValidator<LoginUserCommand> loginValidator, IGoogleAuthService googleAuthService, IUserService userService)
    {
        _registerUserHandler = registerUser;
        _loginUserHandler = loginUserHandler;
        _validator = validator;
        _validationErrorHandler = validationErrorHandler;
        _loginValidator = loginValidator;
        _context = context;
        _googleAuthService = googleAuthService;
        _userService = userService;


    }

    public async Task<IResult> AddGoogleUser(GoogleSignInVM? model)
    {
        if (model == null || model?.IdToken == null) throw new ArgumentException();
        UserDto? response = await _googleAuthService.GoogleSignIn(model);
        if (response is null) throw new BadHttpRequestException("Failed to login through google");
        return Results.Ok(new {message = "The user is registered", user = response});
    }

    public async Task<IResult> AddUser(RegisterUserCommand request)
    {
        var validationResults = await _validator.ValidateAsync(request);
        if (!validationResults.IsValid)
        {
            return Results.ValidationProblem(_validationErrorHandler.validationErrosHandler(validationResults));
        }
        RegisterUserResponse response = await _registerUserHandler.RegisterUserService(request);
        if (String.IsNullOrEmpty(response.FirstName))
        {
            return Results.BadRequest(new { message = "The user is not registered" });
        }

        return Results.Ok(new { message = "The user is registered", data = response });
    }

    public async Task<IResult> LoginUser(LoginUserCommand? request, GoogleSignInVM? model)
    {
        LoginResponse? response = null;
        if (request != null)
        {
            var validationResults = await _loginValidator.ValidateAsync(request);
            if (!validationResults.IsValid)
            {
                return Results.ValidationProblem(_validationErrorHandler.validationErrosHandler(validationResults));
            }
            response = await _loginUserHandler.LoginUserHandler(request);
            
        }

        if (model != null)
        {
            response = await _googleAuthService.GoogleLogIn(model);
        }
        

        var cookieOptions = new CookieOptions()
        {
            IsEssential = true,
            Expires = DateTime.Now.AddDays(7),
            Secure = true,
            HttpOnly = false,
            SameSite = SameSiteMode.None
        };
        _context.HttpContext?.Response.Cookies.Append("token",response.token, cookieOptions);
        return Results.Ok(new {Message = "The user is registered"});

    }

    public async Task<IResult> UpdateUser(UpdateUserCommand updateUserCommand)
    {
        UpdateResponse updateResponse = await _userService.UpdateUserHandler(updateUserCommand);
        if (updateResponse.User.Equals(null))
        {
            return Results.BadRequest("Failed to update user");
        }

        return Results.Ok(updateResponse);
    }

    public async Task<IResult> RetrieveUserHandler(string id)
    {
        var retrievedUser = await _loginUserHandler.RetrieveUser(id);
        if (retrievedUser.Equals(null))
        {
            throw new BadHttpRequestException("Failed to retrieve user");
        }

        return Results.Ok(new { Message = "The user is retrieved", User = retrievedUser });
    }

    public async Task<IResult> GetAllUsers(AllUserCommand request)
    {
        AllUserResponse response = await _userService.getUsersHandler(request);
        if (response.Equals(null))
        {
            return Results.BadRequest("Failed to retrieve the users");
        }

        return Results.Ok(new { message = "The users are retrieved", data = response });
    }

    public async Task<IResult> PromoteUser(string userId)
    {
        if (userId.IsNullOrEmpty())
        {
            return Results.BadRequest("The user id is not found or empty");
        }

        UserDto user = await _userService.promoteUserHandler(userId);
        return Results.Ok(new { message = "The user is promoted", user = user });
    }

    public async Task<IResult> BanUserHandler(string id)
    {
        var result = await _userService.BanUserHandler(id);
        if (!result)
        {
            return Results.BadRequest("Failed to ban User");
        }

        return Results.Ok("The user is Deleted");
    }

    public async Task<IResult> UnBanUserHandler(string id)
    {
        var result = await _userService.BanUserHandler(id);
        if (!result)
        {
            return Results.BadRequest("Failed to ban User");
        }

        return Results.Ok("The user is Deleted");
    }

    public async Task<IResult> DeleteUserHandler(string id)
    {
        var result = await _userService.BanUserHandler(id);
        if (!result)
        {
            return Results.BadRequest("Failed to ban User");
        }

        return Results.Ok("The user is Deleted");
    }
}