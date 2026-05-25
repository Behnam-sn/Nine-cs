using Nine.Identities.Domain.Contracts.Accounts.Enums;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Entities;

public abstract class Credential
{
    protected Credential(CredentialId id, CredentialType type)
    {
        Id = id;
        Type = type;
    }

    public CredentialId Id { get; }
    public CredentialType Type { get; }
}
