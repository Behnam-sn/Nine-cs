using System.Text.RegularExpressions;

using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly partial record struct PhoneNumber
{
    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Regex();

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new PhoneNumberCannotBeEmptyException();
        }

        value = value.Trim();

        string digitsOnly;
        if (value.StartsWith('+'))
        {
            digitsOnly = '+' + new string(value[1..].Where(char.IsDigit).ToArray());
        }
        else
        {
            throw new PhoneNumberInvalidFormatException();
        }

        if (!E164Regex().IsMatch(digitsOnly))
        {
            throw new PhoneNumberInvalidFormatException();
        }

        return new PhoneNumber(digitsOnly);
    }
}
