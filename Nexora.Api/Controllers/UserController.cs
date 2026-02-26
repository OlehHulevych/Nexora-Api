using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Users.Commands.Register;
using Nexora.Infrastructure.Repository;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/user")]
public class UserController:ControllerBase
{
    private readonly UserRepository _userRepository;

    public UserController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    [HttpPost]
    public async Task<IResult> addUserHandler([FromBody] RegisterUserCommand request)
    {
        return await _userRepository.AddUser(request);
    }
}