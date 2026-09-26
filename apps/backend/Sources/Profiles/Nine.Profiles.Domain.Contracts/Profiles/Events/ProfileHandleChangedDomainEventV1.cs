using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.Shared.Domain.Abstractions.Events;
using Nine.Shared.Domain.Abstractions.ValueObjects;

namespace Nine.Profiles.Domain.Contracts.Profiles.Events;

public sealed record ProfileHandleChangedDomainEventV1(
    DomainEventId Id,
    ProfileId ProfileId,
    ProfileHandle Handle,
    DateTime Timestamp
) : IDomainEvent;