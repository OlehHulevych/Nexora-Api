using Nexora.Application.Users.Commands.Login;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IGoogleAuthService
{
    Task<UserDto?> GoogleSignIn(GoogleSignInVM? model);
    Task<LoginResponse> GoogleLogIn(GoogleSignInVM? model);
}