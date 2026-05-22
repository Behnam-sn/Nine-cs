using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly struct Name
{
    const int MaxLength = 100;
    
    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; }

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
