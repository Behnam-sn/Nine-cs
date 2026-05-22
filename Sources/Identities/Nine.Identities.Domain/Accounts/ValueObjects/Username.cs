namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly struct Username
{
    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string value)
    {
        return new(value);
    }
}
