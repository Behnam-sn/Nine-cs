using Marten;

using Nine.Identities.Domain.Accounts.Services;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.Identities.Infrastructure.Accounts.ReadModels;

namespace Nine.Identities.Infrastructure.Accounts.Services;

public sealed class AccountPhoneNumberUniquenessChecker : IAccountPhoneNumberUniquenessChecker
{
    private readonly IQuerySession _querySession;

    public AccountPhoneNumberUniquenessChecker(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public Task<bool> IsTakenAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default)
    {
        return _querySession.Query<AccountPhoneNumberLookup>()
            .AnyAsync(lookup => lookup.PhoneNumber == phoneNumber.Value, cancellationToken);
    }
}
