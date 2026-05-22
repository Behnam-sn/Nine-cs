namespace Nine.Identities.Domain.Accounts.ValueObjects;

public struct FirstName
{
    public FirstName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static FirstName Create(string value)
    {
        return new FirstName(value);
    }
}
