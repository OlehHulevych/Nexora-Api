using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Common;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Update;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Enums;

namespace Nexora.Application.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<bool> AddUser(ApplicationUser user, string? password, LoginProvider? loginProvider, GoogleJsonWebSignature.Payload? payload);
    public Task<bool> UpdateUser(ApplicationUser user);
    public Task<ApplicationUser?> GetUser(string id);
    public Task<PagedResult<ApplicationUser>> GetAllUsers(AllUserCommand request);
    public Task<bool> AddRole(ApplicationUser user, string role);
    public Task<ApplicationUser?> FindByEmail(string email);
    public Task<bool> CheckPassword(ApplicationUser user, string password);
    public Task<bool> DeleteUser(string id);


}