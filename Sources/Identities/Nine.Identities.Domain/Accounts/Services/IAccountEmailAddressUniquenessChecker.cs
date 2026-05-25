using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Services;

public interface IAccountEmailAddressUniquenessChecker
{
    Task<bool> IsTakenAsync(EmailAddress emailAddress, CancellationToken cancellationToken = default);
}
