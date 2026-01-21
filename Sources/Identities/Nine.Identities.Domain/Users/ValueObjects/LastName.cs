namespace Nine.Identities.Domain.Users.ValueObjects;

public struct LastName
{
    public string Value { get; }

    public LastName(string value)
    {
        Value = value;
    }

    public static LastName Create(string value)
    {
        return new LastName(value);
    }
}