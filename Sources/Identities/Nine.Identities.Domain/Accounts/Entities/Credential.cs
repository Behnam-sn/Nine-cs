using Nine.Identities.Domain.Accounts.Enums;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Entities;

public sealed class Credential
{
    private Credential(CredentialId id, CredentialType type, HashedSecret secret)
    {
        Id = id;
        Type = type;
        Secret = secret;
    }

    public CredentialId Id { get; }
    public CredentialType Type { get; }
    public HashedSecret Secret { get; private set; }

    internal void SetSecret(HashedSecret newSecret)
    {
        Secret = newSecret;
    }

    public static Credential CreateInstance(CredentialId id, CredentialType type, HashedSecret secret)
    {
        return new(id, type, secret);
    }
}
