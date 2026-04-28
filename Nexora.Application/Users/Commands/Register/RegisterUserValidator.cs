using System.Data;
using FluentValidation;
using Nexora.Application.Users.Commands.Validation;

namespace Nexora.Application.Users.Commands.Register;


public sealed class RegisterUserValidator:AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(request => request.FirstName).NotEmpty().WithMessage("The first name is  required").NoHmtl().NameValidator().WithMessage("The first name must contain only letters");
        RuleFor(request => request.Password).NotEmpty().WithMessage("The second name is required").ValidatePassword();
        RuleFor(request => request.Email).NotEmpty().WithMessage("The email is required").NoHmtl().EmailAddress().WithMessage("The email must have email format");
        RuleFor(request => request.Address.City).NotEmpty().WithMessage("The city is required").NoHmtl();
        RuleFor(request => request.Address.PostalCode).NotEmpty().WithMessage("The postal code is required");
        RuleFor(request => request.Address.Line1).NotEmpty().WithMessage("The address is required").NoHmtl();
    }
}