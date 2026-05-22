namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly struct Username
{
    private Username(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Username Create(string value)
    {
        return new(value);
    }
}
