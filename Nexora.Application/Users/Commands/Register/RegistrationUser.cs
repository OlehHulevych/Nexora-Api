using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;



namespace Nexora.Application.Users.Commands.Register;

public class RegistrationUser
{
    public UserManager<ApplicationUser> _userManager { get; set; }
  

    public RegistrationUser(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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
            Email = request.Email,
            UserName = request.FirstName + " "+ request.LastName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            

        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (result==null)
        {
            throw new AuthenticationFailureException("Failed to create register");
        }

        Domain.Entities.Address address = new Domain.Entities.Address(user.Id, request.Address.Line1,
            request.Address.City, request.Address.Country, request.Address.PostalCode, request.Address.Line2);

        RegisterUserResponse registerUserResult = new RegisterUserResponse(user.Id, user.Email, user.FirstName, user.LastName, address.Line1);
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