using Nine.Identities.Domain.Contracts.Users.ValueObjects;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Profiles.Domain.Contracts.Profiles.Events;

public sealed record ProfileCreatedDomainEventV1(
    DomainEventId Id,
    ProfileId ProfileId,
    ProfileName Name,
    ProfileHandle Handle,
    ProfileAvatar? Avatar,
    ProfileBio Bio,
    UserId OwnerId,
    DateTime Timestamp
) : IDomainEvent;