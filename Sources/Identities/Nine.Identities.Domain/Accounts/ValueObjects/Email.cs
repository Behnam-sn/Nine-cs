using System.Text.RegularExpressions;

using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly partial record struct Email
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex ValidEmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new EmailCannotBeEmptyException();
        }

        value = value.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(value) || !ValidEmailRegex().IsMatch(value))
        {
            throw new EmailInvalidFormatException();
        }

        return new(value);
    }
}