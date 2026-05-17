using System.Text.RegularExpressions;

using Nine.Identities.Domain.Users.Exceptions;

namespace Nine.Identities.Domain.Users.ValueObjects;

public readonly partial struct Email
{
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

        value = value.Trim();

        return new(value);
    }
}