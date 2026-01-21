using Nine.Identities.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Users.Events;

public sealed record UserEmailChangedDomainEventV1(DomainEventId Id, UserId UserId, Email Email, DateTime OccurredAt)
    : IDomainEvent;