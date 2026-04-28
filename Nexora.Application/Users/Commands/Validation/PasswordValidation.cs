using FluentValidation;

namespace Nexora.Application.Users.Commands.Validation;

public static class PasswordValidation
{
  public static IRuleBuilder<T, string> ValidatePassword<T>(this IRuleBuilderOptions<T, string> rule)
  {
    return rule.NotEmpty().WithMessage("The password is required").NotNull().WithMessage("The password is required")
      .Must(password => password.Any(char.IsUpper))
      .WithMessage("The password must contain atleast one capitalize symbol")
      .Must(password => password.Any(char.IsDigit)).WithMessage("The password contain atleast one digit")
      .Must(password => !ContainsConsecutiveKeyboardLayout(password, PasswordPolicyConstants.MinimumPasswordLength))
      .WithMessage($"The password cannot contain {PasswordPolicyConstants.MaxAlphabeticalSequenceLength} ")
      .Must(password => !ValidateLength(password)).WithMessage(
        $"Password must be at least {PasswordPolicyConstants.MinimumPasswordLength} characters long and meet character type requirements")
      .Must(password =>
        !ContainsAlphabeticalSequence(password, PasswordPolicyConstants.MaxConsecutiveKeyboardCharacters)).WithMessage(
        $"Password cannot contain {PasswordPolicyConstants.MaxConsecutiveKeyboardCharacters} or more consecutive keyboard layout characters");
  }

  private static bool ContainsConsecutiveKeyboardLayout(string password, int length)
  {
    string[] keyboardRows = { "12345678", "qwertyuiop", "asdfghjkl", "zxcvbnm" };
    foreach (var row in keyboardRows)
    {
      for (int i = 0; i <= row.Length - length; i++)
      {
        string sequence = row.Substring(i, length);
        if (password.Contains(sequence, StringComparison.OrdinalIgnoreCase))
        {
          return true;
        }
      }
    }
    return false;
  }

  private static bool ValidateLength(string password)
  {
    if (password.Length < PasswordPolicyConstants.MinimumPasswordLength) return false;
    if (password.Length >= PasswordPolicyConstants.MinimumPasswordLength &&
        password.Length < PasswordPolicyConstants.MediumPasswordLengthThreshold) return false;
    if (password.Length >= PasswordPolicyConstants.MediumPasswordLengthThreshold) return false;
    return true;


  }

  private static bool ContainsAlphabeticalSequence(string password, int length)
  {
    for (int i = 0; i <= password.Length; i++)
    {
      if (password.Skip(i).Take(length).Select(c => char.ToLower(c))
          .SequenceEqual(Enumerable.Range('a', length).Select(x => (char)x)))
      {
        return true;
      }
    }

    return false;
  }
}