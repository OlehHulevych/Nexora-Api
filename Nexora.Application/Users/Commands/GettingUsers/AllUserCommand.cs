using Nexora.Domain.Enums;

namespace Nexora.Application.Users.Commands.GettingUsers;

public record AllUserCommand(
    string? role,
    int currentPage = 1
    );
