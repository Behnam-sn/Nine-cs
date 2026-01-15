using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserNameChangedDomainEventV1(DomainEventId Id, UserId UserId, Name Name, DateTime OccurredAt)
    : IDomainEvent;