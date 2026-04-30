using Microsoft.AspNetCore.Identity;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Update;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Interfaces.Services;

public interface IUserService
{


    public Task<UserDto> promoteUserHandler(string userId);
    public  Task<UpdateResponse> UpdateUserHandler(UpdateUserCommand request);
    public Task<bool> BanUserHandler(String id);
    public Task<bool> UnBanUserHandler(String id);
    public Task<bool> DeleteUserHandler(string id);
    public Task<AllUserResponse> getUsersHandler(AllUserCommand request);

}