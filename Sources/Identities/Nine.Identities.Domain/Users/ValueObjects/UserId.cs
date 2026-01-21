namespace Nine.Identities.Domain.Users.ValueObjects;

public readonly record struct UserId
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    internal static UserId Create()
    {
        return new(Guid.NewGuid());
    }

    public static UserId Parse(Guid value)
    {
        return new(value);
    }
}