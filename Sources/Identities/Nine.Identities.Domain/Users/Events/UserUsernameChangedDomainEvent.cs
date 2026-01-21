using Nine.Identities.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Users.Events;

public record UserUsernameChangedDomainEvent(DomainEventId Id, UserId UserId, Username Username, DateTime OccurredAt)
    : IDomainEvent;