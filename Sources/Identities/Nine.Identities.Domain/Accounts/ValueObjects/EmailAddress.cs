using System.Text.RegularExpressions;

using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly partial record struct EmailAddress
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex ValidEmailAddressRegex();


    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new EmailAddressCannotBeEmptyException();
        }

        value = value.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(value) || !ValidEmailAddressRegex().IsMatch(value))
        {
            throw new EmailAddressInvalidFormatException();
        }

        return new(value);
    }
}
