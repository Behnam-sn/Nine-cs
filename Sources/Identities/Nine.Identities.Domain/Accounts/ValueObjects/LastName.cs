namespace Nine.Identities.Domain.Accounts.ValueObjects;

public struct LastName
{
    public LastName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LastName Create(string value)
    {
        return new LastName(value);
    }
}
