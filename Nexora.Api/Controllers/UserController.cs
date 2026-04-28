using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Update;
using Nexora.Domain.Constants;


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
    public async Task<IResult> AddUserHandler([FromForm] RegisterUserCommand request)
    {
        return await _userRepository.AddUser(request);
    }
    /// <summary>
    /// Login of  User.
    /// </summary>
    /// <returns>JWT token in cookies</returns>
    [Route("/login")]
    [HttpPost]
    public async Task<IResult> LoginUserHandler([FromForm] LoginUserCommand request)
    {
        return await _userRepository.LoginUser(request);
    }
    
    [Authorize]
    [Route("/me")]
    [HttpPost]
    public async Task<IResult> GetUser()
    {
        var Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Id.IsNullOrEmpty())
        {
            return Results.BadRequest(new { Message = "Ivalid id" });
        }
        return await _userRepository.RetrieveUserHandler(Id);
    }
    /// <summary>
    /// Update of  User.
    /// </summary>
    [Authorize]
    [Route("/update")]
    [HttpPost]
    public async Task<IResult> UpdateUser([FromForm]UpdateUserCommand userCommand)
    {
        return await _userRepository.UpdateUser(userCommand);
    }
    [Authorize]
    [Route("/get")]
    [HttpPost]
    public async Task<IResult> GetUsers([FromQuery] AllUserCommand request)
    {
        return await _userRepository.GetAllUsers(request);
    }
    
    [Authorize(Roles = RoleNames.Admin)]
    [Route("/promote")]
    [HttpPatch]
    public async Task<IResult> PromoteUser([FromQuery] string userId)
    {
        return await _userRepository.PromoteUser(userId);
    }
    [Authorize(Roles = RoleNames.Admin)]
    [Route("/ban")]
    [HttpPatch]
    public async Task<IResult> BanUser([FromQuery] string userId)
    {
        return await _userRepository.BanUserHandler(userId);
    }
    [Authorize(Roles = RoleNames.Admin)]
    [Route("/unban")]
    [HttpPatch]
    public async Task<IResult> UnBanUser([FromQuery] string userId)
    {
        return await _userRepository.UnBanUserHandler(userId);
    }
    [Authorize(Roles = RoleNames.Admin)]
    [Route("/delete")]
    [HttpDelete]
    public async Task<IResult> DeleteUser([FromQuery] string userId)
    {
        return await _userRepository.DeleteUserHandler(userId);
    }
    
    
    

}