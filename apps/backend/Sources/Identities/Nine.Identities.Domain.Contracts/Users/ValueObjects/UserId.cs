using Nine.Identities.Domain.Contracts.Users.Exceptions;

namespace Nine.Identities.Domain.Contracts.Users.ValueObjects;

public readonly record struct UserId
{
    private UserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static UserId Create()
    {
        return new(Guid.NewGuid());
    }

    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new UserIdCannotBeEmptyException();
        }

        return new(value);
    }

    public static UserId Parse(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new UserIdInvalidFormatException();
        }

        return From(guid);
    }
}
