using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Infrastructure.Repository;

namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/user")]
public class UserController:ControllerBase
{
    private readonly IUserRepository _userRepository;
    

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
       
    }
    [Route("/register")]
    [HttpPost]
    public async Task<IResult> addUserHandler([FromForm] RegisterUserCommand request)
    {
        return await _userRepository.AddUser(request);
    }

    [Route("/login")]
    [HttpPost]
    public async Task<IResult> loginUserHandler([FromForm] LoginUserCommand request)
    {
        return await _userRepository.LoginUser(request);
    }

}