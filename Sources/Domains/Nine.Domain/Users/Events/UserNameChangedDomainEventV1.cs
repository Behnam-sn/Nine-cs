using Nine.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserNameChangedDomainEventV1(DomainEventId Id, UserId UserId, Name Name, DateTime OccurredAt)
    : IDomainEvent;