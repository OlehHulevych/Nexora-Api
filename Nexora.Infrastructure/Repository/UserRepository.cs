using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Validation;
using Nexora.Infrastructure.Context;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Nexora.Infrastructure.Repository;

public class UserRepository:IUserRepository
{
    private readonly RegistrationUser _registrationUserHandler;
    private readonly LoginUser _loginUserHandler;
    private readonly HttpContext _context;
    private readonly ValidationErrors _validationErrorHandler;
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly IValidator<LoginUserCommand> _loginValidator;

    

    public UserRepository(HttpContext context, RegistrationUser registrationUser, LoginUser loginUserHandler, IValidator<RegisterUserCommand> validator, ValidationErrors validationErrorHandler, IValidator<LoginUserCommand> loginValidator)
    {
        _registrationUserHandler = registrationUser;
        _loginUserHandler = loginUserHandler;
        _validator = validator;
        _validationErrorHandler = validationErrorHandler;
        _loginValidator = loginValidator;
        _context = context;
    }
    public async Task<IResult> AddUser(RegisterUserCommand request)
    {
        var validationResults = await _validator.ValidateAsync(request);
        if (!validationResults.IsValid)
        {
            return Results.ValidationProblem(_validationErrorHandler.validationErrosHandler(validationResults));
        }
        RegisterUserResponse response = await _registrationUserHandler.RegisterUserService(request);
        if (String.IsNullOrEmpty(response.FirstName))
        {
            return Results.BadRequest(new { message = "The user is not registered" });
        }

        return Results.Ok(new { message = "The user is registered", data = response });
    }

    public async Task<IResult> LoginUser(LoginUserCommand request)
    {
        
        var validationResults = await _loginValidator.ValidateAsync(request);
        if (!validationResults.IsValid)
        {
            return Results.ValidationProblem(_validationErrorHandler.validationErrosHandler(validationResults));
        }
        var response = await _loginUserHandler.loginUserHandler(request);
        if (response.token.IsNullOrEmpty())
        {
            return Results.BadRequest();
        }

        var cookieOptions = new CookieOptions()
        {
            IsEssential = true,
            Expires = DateTime.Now.AddDays(7),
            Secure = true,
            HttpOnly = false,
            SameSite = SameSiteMode.None
        };
        _context.Response.Cookies.Append("token",response.token, cookieOptions);
        return Results.Ok(new {Message = "The user is registered"});

    }
}