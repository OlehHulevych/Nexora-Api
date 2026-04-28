namespace Nexora.Application.Users.Commands.Validation;

public class PasswordPolicyConstants
{
    public const int MinimumPasswordLength = 8;
    public const int MediumPasswordLengthThreshold = 10;
    public const int MinimumCharacterTypesForShortPassword = 3;
    public const int MinimumCharacterTypesForLongPassword = 2;

    public const int MaxConsecutiveIdenticalCharacters = 4;
    public const int MaxConsecutiveKeyboardCharacters = 4;
    public const int MaxAlphabeticalSequenceLength = 4;

    public const int SensitiveDataSequenceLength = 3;
    public const int PhoneNumberSequenceLength = 4;
}