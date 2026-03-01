using Microsoft.AspNetCore.Http;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.Register;
using Nexora.Infrastructure.Context;

namespace Nexora.Infrastructure.Repository;

public class UserRepository:IUserRepository
{
    private readonly RegistrationUser _registrationUserHandler;
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context, RegistrationUser registrationUser)
    {
        _registrationUserHandler = registrationUser;
        _context = context;
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