using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Services;

public interface IPasswordHasher
{
    HashedSecret Hash(string value);
}
