using System.Text.RegularExpressions;
using FluentValidation;

namespace Nexora.Application.Users.Commands.Validation;

public static class XssValidation
{
    private static readonly Regex HtmlRegex = new Regex(@"<\s*/?\s*\w+[^>]*>", RegexOptions.Compiled);

    public static IRuleBuilderOptions<T, string> NoHmtl<T>(this IRuleBuilderOptions<T, string> rule)
    {
        return rule.Must(v => v is null || !HtmlRegex.IsMatch(v)).WithMessage("The field must contain no html");
    }
}