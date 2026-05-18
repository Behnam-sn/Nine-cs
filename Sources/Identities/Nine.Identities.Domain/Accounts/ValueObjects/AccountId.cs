namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly record struct AccountId
{
    public Guid Value { get; }

    private AccountId(Guid value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static AccountId Create()
    {
        return new(Guid.NewGuid());
    }

    public static AccountId Parse(Guid value)
    {
        return new(value);
    }
}