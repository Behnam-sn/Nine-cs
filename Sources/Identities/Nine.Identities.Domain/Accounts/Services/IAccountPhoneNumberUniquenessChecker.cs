using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Services;

public interface IAccountPhoneNumberUniquenessChecker
{
    Task<bool> IsTakenAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
}
