using Nine.Identity.Domain.Contracts.Users.Exceptions;

namespace Nine.Identity.Domain.Contracts.Users.ValueObjects;

public readonly record struct PlainPassword
{
    public const int MinimumLength = 8;

    private PlainPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PlainPassword Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new PasswordEmptyException();
        }

        if (value.Length < MinimumLength)
        {
            throw new PasswordTooShortException(MinimumLength);
        }

        if (value.Any(char.IsWhiteSpace))
        {
            throw new PasswordHavingWhiteSpaceException();
        }

        if (!value.Any(char.IsUpper))
        {
            throw new PasswordMissingUppercaseException();
        }

        if (!value.Any(char.IsLower))
        {
            throw new PasswordMissingLowercaseException();
        }

        if (!value.Any(char.IsDigit))
        {
            throw new PasswordMissingDigitException();
        }

        if (value.All(char.IsLetterOrDigit))
        {
            throw new PasswordMissingSpecialCharacterException();
        }

        return new(value);
    }
}
