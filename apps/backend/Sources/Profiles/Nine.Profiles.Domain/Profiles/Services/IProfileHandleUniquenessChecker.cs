using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Domain.Profiles.Services;

public interface IProfileHandleUniquenessChecker
{
    Task<bool> IsTakenAsync(ProfileHandle handle, CancellationToken cancellationToken = default);
}
