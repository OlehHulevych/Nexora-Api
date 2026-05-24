using FluentValidation;
using FluentValidation.Validators;
using Nexora.Application.Product.Command;
using Nexora.Application.Users.Commands.Validation;

namespace Nexora.Application.Product.Validation;

public sealed class ProductFormValidator:AbstractValidator<CreateProductCommand>
{
    public ProductFormValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithMessage("The  name is  required").NoHmtl().NameValidator().WithMessage("The first name must contain only letters");
        RuleFor(request => request.Category).NotEmpty().WithMessage("category is required");
        RuleFor(request => request.Price).NotNull().NotEqual(0).WithMessage("The price is required");
        
    }
}