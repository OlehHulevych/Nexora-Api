using Microsoft.AspNetCore.Http;
using Nexora.Application.Users.Commands.Login;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Services;

public interface IGoogleAuthService
{
    Task<IResult> GoogleSignIn(GoogleSignInVM? model);
    Task<IResult> GoogleLogIn(GoogleSignInVM? model);
}