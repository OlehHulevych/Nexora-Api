using Microsoft.AspNetCore.Http;
using Nexora.Application.Addresses;

namespace Nexora.Application.Users.Commands.Update;

public record UpdateUserCommand ( 
    string? Firstname,
    string? LastName,
    IFormFile? Photo,
    string? Email,
    AddressCommand? Address
    );