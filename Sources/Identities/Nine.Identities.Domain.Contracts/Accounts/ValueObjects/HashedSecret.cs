using Nine.Identities.Domain.Contracts.Accounts.Exceptions;

namespace Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

public readonly record struct HashedSecret
{
    private HashedSecret(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public string Value { get; }

    public static HashedSecret Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new HashedSecretCannotBeEmptyException();
        }

        return new(value);
    }
}
