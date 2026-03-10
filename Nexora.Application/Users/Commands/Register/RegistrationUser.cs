using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;



namespace Nexora.Application.Users.Commands.Register;

public class RegistrationUser
{
    private ILogger<RegistrationUser> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly UploadAvatarHandler _uploadAvatar;
  

    public RegistrationUser( IApplicationDbContext context, UserManager<ApplicationUser> userManager, UploadAvatarHandler uploadAvatar, ILogger<RegistrationUser> logger)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
        _uploadAvatar = uploadAvatar;
    }
    
    public async Task<RegisterUserResponse> RegisterUserService(RegisterUserCommand request)
    {
            bool userExist = await CheckIfuserExistByEmail(request.Email);
            if (userExist)
            {
                throw new UserAlreadyExistsException(request.Email);
            }
            var matchPassword = MatchingPasswordHandler(request.Password, request.ConfirmPassword);
            if (!matchPassword)
            {
                throw new PasswordIsNotMatched();
            }
            ApplicationUser user = new ApplicationUser()
            {
                UserName = request.FirstName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,


            };
            var result = await _userManager.CreateAsync(user, request.Password);
            Domain.Entities.Address address = new Domain.Entities.Address(user.Id, request.Address.Line1,
                request.Address.City, request.Address.Country, request.Address.PostalCode, request.Address.Line2);
            await _context.Addresses.AddAsync(address);
            UploadAvatarResponse avatarResponse =
                await _uploadAvatar.UploadAvatar(new UploadAvatarCommand(user.Id, user, request.Avatar), request.FirstName+"_"+request.LastName);
            if (avatarResponse.uri.IsNullOrEmpty())
            {
                throw new Exception("Register failed during the uploading avatar");
            }
            user.Avatar = avatarResponse.Avatar;
            user.Address = address;
            await _context.SaveChangesAsync();
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new AuthenticationFailureException(errors); 
            }
            RegisterUserResponse registerUserResult = new RegisterUserResponse(user.Id, user.Email, user.FirstName, user.LastName, address.Line1);
            _logger.LogInformation("The user is registered");
            return registerUserResult;

    }

    public async Task<bool> CheckIfuserExistByEmail(string email)
    {
        var result = await _userManager.FindByEmailAsync(email);
        return result == null && false;
    }

    public bool MatchingPasswordHandler(string password, string confirmPassword)
    {
        return password.Equals(confirmPassword);
    }
}