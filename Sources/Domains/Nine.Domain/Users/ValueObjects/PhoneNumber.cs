namespace Nine.Domain.Users.ValueObjects;

public readonly struct PhoneNumber
{
    public string Value { get; }
    
    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        return new(value);
    }
}