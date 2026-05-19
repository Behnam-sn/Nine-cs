using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly record struct HashedSecret
{
    public string Value { get; }

    private HashedSecret(string value)
    {
        Value = value;
    }
    
    public override string ToString()
    {
        return Value;
    }

    public static HashedSecret Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new HashedSecretCannotBeEmptyException();
        }
        
        return new (input.Trim());
    }
}