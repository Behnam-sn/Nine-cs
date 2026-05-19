using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly record struct CredentialId
{
    public Guid Value { get; }

    private CredentialId(Guid value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static CredentialId Create()
    {
        return new(Guid.NewGuid());
    }

    public static CredentialId From(Guid input)
    {
        if (input == Guid.Empty)
        {
            throw new CredentialIdCannotBeEmptyException();
        }

        return new(input);
    }
}