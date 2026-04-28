namespace Nexora.Application.Users.Commands.Register;
using Nexora.Domain.Entities;
public record RegisterUserResponse(
        string UserId,
        string Email,
        string FirstName,
        string LastName,
        string Address

    );