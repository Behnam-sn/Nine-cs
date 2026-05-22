using Nine.Identities.Domain.Contracts.Accounts.Exceptions;

namespace Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

public readonly record struct CredentialId
{
    private CredentialId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

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

    public static CredentialId Parse(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new CredentialIdInvalidFormatException();
        }

        return From(guid);
    }
}
