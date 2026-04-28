using System.Text.RegularExpressions;
using FluentValidation;

namespace Nexora.Application.Users.Commands.Validation;

public static class NameValidation
{
    private static readonly Regex NameRegex = new Regex(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$", RegexOptions.Compiled);

    public static IRuleBuilderOptions<T, string> NameValidator<T>(this IRuleBuilderOptions<T, string> rule)
    {
        return rule.Must(name => NameRegex.IsMatch(name));
    }
}