using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserCreatedDomainEventV1(
    DomainEventId Id,
    UserId UserId,
    FirstName FirstName,
    LastName LastName,
    Email Email,
    DateTime OccurredAt
) : IDomainEvent;