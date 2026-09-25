using Marten;
using Nine.Profiles.Domain.Profiles.Entities;
using Nine.Profiles.Domain.Profiles.Repositories;

namespace Nine.Profiles.Infrastructure.Profiles.Repositories;

public sealed class ProfileCommandRepository : IProfileCommandRepository
{
    private readonly IDocumentSession _session;

    public ProfileCommandRepository(IDocumentSession session)
    {
        _session = session;
    }

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        var uncommittedEvents = profile.DomainEvents.ToArray();
        if (uncommittedEvents.Length == 0)
        {
            return;
        }

        _session.Events.StartStream<Profile>(profile.ProfileId.Value, uncommittedEvents);
        await _session.SaveChangesAsync(cancellationToken);
        profile.ClearDomainEvents();
    }
}