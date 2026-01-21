namespace Nine.Identities.Domain.Users.ValueObjects;

public readonly struct Username
{
    private string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string value)
    {
        return new(value);
    }
}