using Marten.Events.Aggregation;
using Nine.Profiles.Domain.Contracts.Profiles.Events;
using Nine.Profiles.Infrastructure.Profiles.ReadModels;

namespace Nine.Profiles.Infrastructure.Profiles.Projections;

public sealed class ProfileHandleLookupProjection : SingleStreamProjection<ProfileHandleLookup, Guid>
{
    public ProfileHandleLookup Create(ProfileCreatedDomainEventV1 domainEvent)
    {
        return new ProfileHandleLookup
        {
            Id = domainEvent.ProfileId.Value,
            Handle = domainEvent.Handle.Value
        };
    }

    public void Apply(ProfileHandleChangedDomainEventV1 domainEvent, ProfileHandleLookup lookup)
    {
        lookup.Handle = domainEvent.Handle.Value;
    }
}
