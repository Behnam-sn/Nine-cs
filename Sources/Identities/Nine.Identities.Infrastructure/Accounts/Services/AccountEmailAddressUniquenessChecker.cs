using Marten;

using Nine.Identities.Domain.Accounts.Services;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.Identities.Infrastructure.Accounts.ReadModels;

namespace Nine.Identities.Infrastructure.Accounts.Services;

public sealed class AccountEmailAddressUniquenessChecker : IAccountEmailAddressUniquenessChecker
{
    private readonly IQuerySession _querySession;

    public AccountEmailAddressUniquenessChecker(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public Task<bool> IsTakenAsync(EmailAddress emailAddress, CancellationToken cancellationToken = default)
    {
        return _querySession.Query<AccountEmailAddressLookup>()
            .AnyAsync(lookup => lookup.EmailAddress == emailAddress.Value, cancellationToken);
    }
}
