using Nine.Domain.Abstractions.Events;

namespace Nine.Domain.Users.Events;

public sealed record UserFirstNameChangedDomainEventV1(Guid Id, Guid UserId, string FirstName, DateTime OccurredAt)
    : IDomainEvent;