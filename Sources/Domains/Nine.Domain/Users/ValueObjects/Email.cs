namespace Nine.Domain.Users.ValueObjects;

public readonly struct Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        return new(value);
    }
}