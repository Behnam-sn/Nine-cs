using Nine.Identities.Domain.Users.Exceptions;

namespace Nine.Identities.Domain.Users.ValueObjects;

public readonly struct PhoneNumber
{
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
        return new(value);
    }
}