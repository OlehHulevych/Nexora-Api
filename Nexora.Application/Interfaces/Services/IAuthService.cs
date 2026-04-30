using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Interfaces.Services;

public interface IAuthService
{
    public Task<RegisterUserResponse> RegisterUserService(RegisterUserCommand request);

    public Task<LoginResponse> LoginUserHandler(LoginUserCommand request);

    public Task<UserDto> RetrieveUser(string id);
}