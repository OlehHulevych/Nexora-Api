using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Nexora.Application.Users.Commands.Validation;

public class ValidationErrors
{
    public Dictionary<string, string[]> validationErrosHandler(ValidationResult validationResults)
    {
        var errors = validationResults.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return errors;
    }
}