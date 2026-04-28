using Microsoft.AspNetCore.Http;
using Nexora.Application.Address;

namespace Nexora.Application.Users.Commands.Update;

public record UpdateUserCommand ( 
    string Id,
    string? Firstname,
    string? LastName,
    IFormFile? Photo,
    string? Email,
    AddressCommand? Address
    );