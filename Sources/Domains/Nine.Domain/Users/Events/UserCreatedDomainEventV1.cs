using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserCreatedDomainEventV1(DomainEventId Id, string FirstName, string LastName, DateTime OccurredAt)
    : IDomainEvent;