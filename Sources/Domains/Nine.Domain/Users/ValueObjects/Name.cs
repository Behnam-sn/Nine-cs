namespace Nine.Domain.Users.ValueObjects;

public readonly struct Name
{
    public string Value { get; }

    private Name(string value)
    {
        Value = value;
    }

    public static Name Create(string value)
    {
        return new(value);
    }
}