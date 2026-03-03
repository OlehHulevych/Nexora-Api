using Microsoft.AspNetCore.Http;
using Nexora.Application.Address;

namespace Nexora.Application.Users.Commands.Register;

public record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword,
    IFormFile Avatar,
    AddressCommand Address
    );