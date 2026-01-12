using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Entities;

public record UserUsernameChangedDomainEvent(DomainEventId Id, UserId UserId, Username Username, DateTime OccurredAt)
    : IDomainEvent;