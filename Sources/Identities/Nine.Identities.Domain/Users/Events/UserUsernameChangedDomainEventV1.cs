using Nine.Identities.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Users.Events;

public sealed record UserUsernameChangedDomainEventV1(DomainEventId Id, UserId UserId, Username Username, DateTime OccurredAt)
    : IDomainEvent;