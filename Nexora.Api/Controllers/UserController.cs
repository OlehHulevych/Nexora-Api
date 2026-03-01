using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.Register;
using Nexora.Infrastructure.Repository;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/user")]
public class UserController:ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<RegisterUserCommand> _validator;

    public UserController(IUserRepository userRepository, IValidator<RegisterUserCommand> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }
    [Route("/register")]
    [HttpPost]
    public async Task<IResult> addUserHandler([FromForm] RegisterUserCommand request)
    {
        var validationResults = await _validator.ValidateAsync(request);
        if (!validationResults.IsValid)
        {
            var errors = validationResults.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }
        return await _userRepository.AddUser(request);
    }
}