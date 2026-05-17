using Nine.Identities.Domain.Users.Exceptions;

namespace Nine.Identities.Domain.Users.ValueObjects;

public readonly struct Name
{
    const int MaxLength = 100;

    public string Value { get; }

    private Name(string value)
    {
        Value = value;
    }

    public static Name Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new NameCannotBeEmptyException();
        }
        
        value = value.Trim();

        if (value.Length > MaxLength)
        {
            throw new NameTooLongException();
        }

        return new(value);
    }
}