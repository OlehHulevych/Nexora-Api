namespace Nexora.Application.Users.Commands.Login;

public record LoginUserCommand(
    string email,
    string password
    );