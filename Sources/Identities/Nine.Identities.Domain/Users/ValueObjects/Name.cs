using Nine.Identities.Domain.Users.Exceptions;

namespace Nine.Identities.Domain.Users.ValueObjects;

public readonly struct Name
{
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
        
        return new(value);
    }
}