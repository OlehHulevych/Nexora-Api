using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Nexora.Application.Users.Services;

public class AuthService:IAuthService
{
    private readonly IAvatarService _avatarService;
    private readonly IHttpContextAccessor _httpAccessor;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IFavoriteListRepository _favoriteListRepository;
    
    private readonly ILogger<AuthService> _logger;

    public AuthService( IAvatarService avatarService, ILogger<AuthService> logger, 
        IJwtService jwtService,IUserRepository userRepository, IHttpContextAccessor httpAccessor, IAddressRepository addressRepository, ICartRepository cartRepository, IFavoriteListRepository favoriteListRepository)
    {
        _avatarService = avatarService;
        _logger = logger;
        _jwtService = jwtService;
        _httpAccessor = httpAccessor;
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _cartRepository = cartRepository;
        _favoriteListRepository = favoriteListRepository;
    }
    
    public async Task<IResult> RegisterUserService(RegisterUserCommand request)
{
    _logger.LogInformation("Registering user with email {Email}", request.Email);
    
    bool userExist = await CheckIfUserExistByEmail(request.Email);
    if (userExist)
    {
        _logger.LogWarning("Registration failed — user {Email} already exists", request.Email);
        throw new UserAlreadyExistsException(request.Email);
    }
    
    var matchPassword = MatchingPasswordHandler(request.Password, request.ConfirmPassword);
    if (!matchPassword)
    {
        _logger.LogWarning("Registration failed — passwords do not match for {Email}", request.Email);
        throw new PasswordIsNotMatched();
    }
    
    ApplicationUser user = new ApplicationUser()
    {
        UserName = request.FirstName,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
    };
    
    var result = await _userRepository.AddUser(user, request.Password, null, null);
    if (!result)
    {
        _logger.LogError("Failed to create user {Email} in database", request.Email);
        throw new BadHttpRequestException("Failed to create new user");
    }
    _logger.LogInformation("User {Email} created successfully with Id {UserId}", request.Email, user.Id);
    
    _logger.LogInformation("Creating address for user {UserId}", user.Id);
    Address address = new Address(user.Id, request.Address.Line1,
        request.Address.City, request.Address.Country, request.Address.PostalCode, request.Address.Line2);
    await _addressRepository.AddAsync(address);
    _logger.LogInformation("Address created for user {UserId}", user.Id);
    
    var resultRoles = await _userRepository.AddRole(user, RoleNames.User);
    if (!resultRoles)
    {
        _logger.LogError("Failed to assign role {Role} to user {UserId}", RoleNames.User, user.Id);
        throw new Exception("Failed to load role to user");
    }
    _logger.LogInformation("Role {Role} assigned to user {UserId}", RoleNames.User, user.Id);
    
    _logger.LogInformation("Uploading avatar for user {UserId}", user.Id);
    UploadAvatarResponse avatarResponse =
        await _avatarService.UploadAvatar(new UploadAvatarCommand(user.Id, user, request.Avatar), request.FirstName + "_" + request.LastName);
    if (avatarResponse.uri.IsNullOrEmpty())
    {
        _logger.LogError("Avatar upload failed for user {UserId}", user.Id);
        throw new Exception("Register failed during the uploading avatar");
    }
    _logger.LogInformation("Avatar uploaded for user {UserId}: {AvatarUri}", user.Id, avatarResponse.uri);
    
    _logger.LogInformation("Creating cart for user {UserId}", user.Id);
    Cart newCart = new Cart { UserId = user.Id };
    await _cartRepository.Add(newCart);
    _logger.LogInformation("Cart created for user {UserId}", user.Id);
    
    _logger.LogInformation("Creating favorite list for user {UserId}", user.Id);
    FavoriteList favoriteList = new FavoriteList
    {
        UserId = user.Id,
        User = user,
        FavoriteItems = new List<FavoriteItem>()
    };
    await _favoriteListRepository.Add(favoriteList);
    _logger.LogInformation("Favorite list created for user {UserId}", user.Id);
    
    user.Avatar = avatarResponse.Avatar;
    user.Address = address;
    user.Cart = newCart;
    user.FavoriteList = favoriteList;
    await _userRepository.UpdateUser(user);
    
    _logger.LogInformation("User {Email} registered successfully with Id {UserId}", request.Email, user.Id);
    RegisterUserResponse registerUserResult = new RegisterUserResponse(user.Id, user.Email, user.FirstName, user.LastName, address.Line1);
    return Results.Ok(new { message = "The user was registered", data = registerUserResult });
}

public async Task<IResult> LoginUserHandler(LoginUserCommand? request)
{
    _logger.LogInformation("Login attempt for email {Email}", request?.email);
    
    if (request != null && (request.email.IsNullOrEmpty() || request.password.IsNullOrEmpty()))
    {
        _logger.LogWarning("Login failed — missing email or password");
        throw new ValidationException("Required data is missing");
    }

    if (request != null)
    {
        var user = await _userRepository.FindByEmail(request.email);
        if (user == null)
        {
            _logger.LogWarning("Login failed — user {Email} not found", request.email);
            throw new UserIsNotFoundException();
        }

        var result = await _userRepository.CheckPassword(user, request.password);
        if (!result)
        {
            _logger.LogWarning("Login failed — wrong password for user {Email}", request.email);
            throw new PasswordIsNotMatched();
        }

        string? token = await _jwtService.CreateToken(user);
        if (token.IsNullOrEmpty())
        {
            _logger.LogError("Failed to generate JWT token for user {Email}", request.email);
            throw new BadHttpRequestException("Failed during creating token");
        }

        if (token != null)
        {
            _httpAccessor.HttpContext?.Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            _logger.LogInformation("User {Email} logged in successfully", request.email);
            return Results.Ok(new { message = "The user was logged" });
        }
    }

    _logger.LogError("Login failed for unknown reason");
    return Results.BadRequest(new { message = "failed to register user" });
}

public async Task<IResult> RetrieveUser(string id)
{
    _logger.LogInformation("Fetching user with Id {UserId}", id);
    
    if (id.IsNullOrEmpty())
    {
        _logger.LogWarning("Retrieve user failed — missing user Id");
        throw new BadHttpRequestException("There is no user's id");
    }

    var user = await _userRepository.FindById(id);
    if (user == null)
    {
        _logger.LogWarning("Retrieve user failed — user {UserId} not found", id);
        throw new UserIsNotFoundException();
    }

    _logger.LogInformation("User {UserId} fetched successfully", id);
    
    if (user.Email != null)
        return Results.Ok(new
        {
            message = "User is fetched", data = new UserDto(user.Id, user.FirstName + " " + user.LastName,
                user.Email, user.Avatar?.Uri,
                user.Address?.Line1 + ", " + user.Address?.City + ", " + user.Address?.Country)
        });
    
    _logger.LogError("User {UserId} has no email", id);
    return Results.BadRequest(new { message = "Failed to retrieve user" });
}

    private async Task<bool> CheckIfUserExistByEmail(string email)
    {
        var result = await _userRepository.CheckUserIfExistByEmail(email);
        return result;
    }

    private bool MatchingPasswordHandler(string password, string confirmPassword)
    {
        return password.Equals(confirmPassword);
    }
}