using Nine.Identities.Domain.Contracts.Accounts.Enums;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Entities;

public sealed class PasswordCredential : Credential
{
    internal PasswordCredential(CredentialId id, HashedSecret hashedPassword)
        : base(id, CredentialType.Password)
    {
        HashedPassword = hashedPassword;
    }

    public HashedSecret HashedPassword { get; private set; }

    internal void SetHashedPassword(HashedSecret newHashedPassword)
    {
        HashedPassword = newHashedPassword;
    }
}
