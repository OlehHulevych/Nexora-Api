using Microsoft.AspNetCore.Http;
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


    public Task<IResult> PromoteUserHandler(string userId);
    public  Task<IResult> UpdateUserHandler(UpdateUserCommand request);
    public Task<IResult> BanUserHandler(String id);
    public Task<IResult> UnBanUserHandler(String id);
    public Task<IResult> DeleteUserHandler(string id);
    public Task<IResult> GetUsersHandler(AllUserCommand request);

}