using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserEmailChangedDomainEventV1(DomainEventId Id, UserId UserId, Email Email, DateTime OccurredAt)
    : IDomainEvent;