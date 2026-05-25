using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Update;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;


namespace Nexora.Api.Controllers;
[ApiController]
[Route("api/user")]
public class UserController:ControllerBase
{
    private readonly IAuthService _authService ;
    private readonly IUserService _userService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<UserController> _logger;
    

    public UserController(IAuthService authService, IUserService userService, IGoogleAuthService googleAuthService,ILogger<UserController> logger )
    {
        _authService = authService;
        _userService = userService;
        _googleAuthService = googleAuthService;
        _logger = logger;
    }
    /// <summary>
    /// Registration of  User.
    /// </summary>
    /// <param name="request">needed information for registering</param>
    /// <returns>New registered user</returns>
    /// <response code="200">Registered successfully</response>
    /// <response code="404">Some data is missing or user is existing</response>
    
    [Route("/register")]
    [HttpPost]
    public async Task<IResult> AddUserHandler([FromForm] RegisterUserCommand request)
    {
        _logger.LogInformation("User controller - registration of user");
        return await _authService.RegisterUserService(request);
    }
    /// <summary>
    /// Login of  User.
    /// </summary>
    /// <param name="request">email and password</param>
    /// <returns>JWT token in cookies</returns>
    /// <response code="200">Authorized successfully</response>
    /// <response code="404">Some data is missing or user is not existing</response>
    [Route("/login")]
    [HttpPost]
    public async Task<IResult> LoginUserHandler([FromForm] LoginUserCommand? request)
    {
        _logger.LogInformation("User controller - user is logging in");
        return await _authService.LoginUserHandler(request);
    }
    /// <summary>
    /// Registration of  User via Google.
    /// </summary>
    /// <param name="model">contains google token</param>
    /// <returns>created user</returns>
    /// <response code="200">Registered successfully</response>
    /// <response code="404">Some data is missing or user is not existing</response>
    [Route("/register/google")]
    [HttpPost]
    public async Task<IResult> RegisterThroughGoogle([FromBody] GoogleSignInVM model)
    {
        _logger.LogInformation("User controller - registration via google");
        return await _googleAuthService.GoogleSignIn(model);
    }
    /// <summary>
    /// Login of  User via Google.
    /// </summary>
    /// <param name="model">contains google token</param>
    /// <returns>JWT token in cookies</returns>
    /// <response code="200">Authorized successfully</response>
    /// <response code="404">Some data is missing or user is not existing</response>
    [Route("/login/google")]
    [HttpPost]
    public async Task<IResult> LoginThroughGoogle([FromBody] GoogleSignInVM model)
    {
        _logger.LogInformation("User controller - logging in via google");
        return await _googleAuthService.GoogleLogIn(model);
    }
    /// <summary>
    /// Retrieving user via token
    /// </summary>
    /// <returns>authorized user</returns>
    /// <response code="200">Authorized successfully</response>
    /// <response code="404">User is not existed or not logged in</response>
    [Authorize]
    [Route("/me")]
    [HttpPost]
    public async Task<IResult> GetUser()
    {
        _logger.LogInformation("User controller - retrieving user ");
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id.IsNullOrEmpty() || id==null)
        {
            _logger.LogError("User is unauthorized");
            return Results.BadRequest(new { Message = "Invalid id" });
        }
        return await _authService.RetrieveUser(id);
    }
    /// <summary>
    /// Update of  User.
    /// </summary>
    /// <returns>updated user</returns>
    /// <response code="200">updated successfully</response>
    /// <response code="404">User is not existed </response>
    [Authorize]
    [Route("/update")]
    [HttpPost]
    public async Task<IResult> UpdateUser([FromForm]UpdateUserCommand userCommand)
    {
        _logger.LogInformation("User controller - updating user ");
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            _logger.LogError("User is unauthorized");
            throw new UnauthorizedAccessException();
        }

        return await _userService.UpdateUserHandler(id,userCommand);
    }
    /// <summary>
    /// Get users 
    /// </summary>
    /// <returns>users</returns>
    /// <response code="200">users are fetched successfully</response>
    /// <response code="403">Forbiddend access </response>
    [Authorize]
    [Route("/get")]
    [HttpPost]
    public async Task<IResult> GetUsers([FromQuery] AllUserCommand request)
    {
        _logger.LogInformation("User controller - getting all users");
        return await _userService.GetUsersHandler(request);
    }
    /// <summary>
    /// Promoting users (only for admin).
    /// </summary>
    /// <returns>Promoted user</returns>
    /// <response code="200">user is promoted</response>
    /// <response code="403">Forbidden access </response>
    /// <response code="404">User is not existing </response>

    [Authorize(Roles = RoleNames.Admin)]
    [Route("/promote")]
    [HttpPatch]
    public async Task<IResult> PromoteUser([FromQuery] string userId)
    {
        _logger.LogInformation("User controller - promoting user");
        return await _userService.PromoteUserHandler(userId);
    }
    /// <summary>
    /// Banning users (only for admin).
    /// </summary>
    /// <returns>Banned user</returns>
    /// <response code="200">user is banned</response>
    /// <response code="403">Forbidden access </response>
    /// <response code="404">User is not existing </response>

    [Authorize(Roles = RoleNames.Admin)]
    [Route("/ban")]
    [HttpPatch]
    public async Task<IResult> BanUser([FromQuery] string userId)
    {
        _logger.LogInformation("User controller - banning user");
        return await _userService.BanUserHandler(userId);
    }
    /// <summary>
    /// UnBanning users (only for admin).
    /// </summary>
    /// <returns>Unbanned user</returns>
    /// <response code="200">user is unbanned</response>
    /// <response code="403">Forbidden access </response>
   /// <response code="404">User is not existing </response>

    [Authorize(Roles = RoleNames.Admin)]
    [Route("/unban")]
    [HttpPatch]
    public async Task<IResult> UnBanUser([FromQuery] string userId)
    {
        _logger.LogInformation("User controller - unbanning user");
        return await _userService.UnBanUserHandler(userId);
    }
    /// <summary>
    /// Deleting users (only for admin).
    /// </summary>
    /// <response code="200">user is deleted</response>
    /// <response code="403">Forbidden access </response>
    /// <response code="404">User is not existing </response>

    [Authorize(Roles = RoleNames.Admin)]
    [Route("/delete")]
    [HttpDelete]
    public async Task<IResult> DeleteUser([FromQuery] string userId)
    {
        return await _userService.DeleteUserHandler(userId);
    }
    
    
    

}