using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;


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
    /// <summary>
    /// Registration of  User.
    /// </summary>
    /// <returns>New registered user</returns>
    
    [Route("/register")]
    [HttpPost]
    public async Task<IResult> addUserHandler([FromForm] RegisterUserCommand request)
    {
        return await _userRepository.AddUser(request);
    }
    /// <summary>
    /// Login of  User.
    /// </summary>
    /// <returns>JWT token in cookies</returns>
    [Route("/login")]
    [HttpPost]
    public async Task<IResult> loginUserHandler([FromForm] LoginUserCommand request)
    {
        return await _userRepository.LoginUser(request);
    }
    
    [Authorize]
    [Route("/me")]
    [HttpPost]
    public async Task<IResult> getUser()
    {
        var Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Id.IsNullOrEmpty())
        {
            return Results.BadRequest(new { Message = "Ivalid id" });
        }
        return await _userRepository.RetrieveUserHandler(Id);
    }

}