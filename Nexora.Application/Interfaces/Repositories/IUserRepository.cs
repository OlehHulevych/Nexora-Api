using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Update;

namespace Nexora.Application.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<IResult> AddUser(RegisterUserCommand request);
    public Task<IResult> LoginUser(LoginUserCommand request);
    public Task<IResult> UpdateUser(UpdateUserCommand updateUserCommand);

    public Task<IResult> RetrieveUserHandler(string id);
    public Task<IResult> GetAllUsers(AllUserCommand request);

}