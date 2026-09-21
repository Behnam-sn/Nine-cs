using Nine.Profiles.Domain.Profiles.Entities;

namespace Nine.Profiles.Domain.Profiles.Repositories;

public interface IProfileCommandRepository
{
    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);
}
