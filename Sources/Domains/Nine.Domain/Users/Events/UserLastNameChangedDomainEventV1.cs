using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserLastNameChangedDomainEventV1(DomainEventId Id, UserId UserId, LastName LastName, DateTime OccurredAt)
    : IDomainEvent;