using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Users.Commands.Register;

namespace Nexora.Application.Interfaces.Repositories;

public interface IUserRepository
{
    public Task<IResult> AddUser(RegisterUserCommand request);

}