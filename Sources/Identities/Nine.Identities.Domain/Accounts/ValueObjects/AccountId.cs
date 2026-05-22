using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly record struct AccountId
{
    private AccountId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static AccountId Create()
    {
        return new(Guid.NewGuid());
    }

    public static AccountId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new AccountIdCannotBeEmptyException();
        }

        return new(value);
    }

    public static AccountId Parse(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new AccountIdInvalidFormatException();
        }

        return From(guid);
    }
}
