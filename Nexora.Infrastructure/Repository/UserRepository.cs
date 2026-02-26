using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.Register;

namespace Nexora.Infrastructure.Repository;

public class UserRepository:IUserRepository
{
    public RegistrationUser _registrationUserHandler;

    public UserRepository(RegistrationUser registrationUser)
    {
        _registrationUserHandler = registrationUser;
    }
    public async Task<IResult> AddUser(RegisterUserCommand request)
    {
        RegisterUserResponse response = await _registrationUserHandler.RegisterUserService(request);
        if (String.IsNullOrEmpty(response.FirstName))
        {
            return Results.BadRequest(new { message = "The user is not registered" });
        }

        return Results.Ok(new { message = "The user is registered", data = response });
    }
}