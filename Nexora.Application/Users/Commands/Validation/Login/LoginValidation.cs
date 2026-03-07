using FluentValidation;
using Nexora.Application.Users.Commands.Login;

namespace Nexora.Application.Users.Commands.Validation.Login;

public class LoginValidation:AbstractValidator<LoginUserCommand>
{
    public LoginValidation()
    {
        RuleFor(request => request.email).EmailAddress().NotEmpty().NotNull();
        RuleFor(request => request.password).NotEmpty().NotNull();
    }
}