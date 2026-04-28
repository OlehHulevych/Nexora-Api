using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Update;
using Nexora.Domain.DTOs;

namespace Nexora.Application.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<IResult> AddGoogleUser(GoogleSignInVM? model);
    public Task<IResult> AddUser(RegisterUserCommand request);
    public Task<IResult> LoginUser(LoginUserCommand? request=null, GoogleSignInVM? model=null);
    public Task<IResult> UpdateUser(UpdateUserCommand updateUserCommand);

    public Task<IResult> RetrieveUserHandler(string id);
    public Task<IResult> GetAllUsers(AllUserCommand request);
    public Task<IResult> PromoteUser(string userId);
    public Task<IResult> BanUserHandler(String id);
    public Task<IResult> UnBanUserHandler(String id);
    public Task<IResult> DeleteUserHandler(string id);


}