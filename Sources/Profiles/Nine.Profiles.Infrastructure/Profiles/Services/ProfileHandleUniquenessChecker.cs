using Marten;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.Profiles.Domain.Profiles.Services;
using Nine.Profiles.Infrastructure.Profiles.ReadModels;

namespace Nine.Profiles.Infrastructure.Profiles.Services;

public sealed class ProfileHandleUniquenessChecker : IProfileHandleUniquenessChecker
{
    private readonly IQuerySession _querySession;

    public ProfileHandleUniquenessChecker(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public Task<bool> IsTakenAsync(ProfileHandle handle, CancellationToken cancellationToken = default)
    {
        return _querySession.Query<ProfileHandleLookup>()
            .AnyAsync(lookup => lookup.Handle == handle.Value, cancellationToken);
    }
}