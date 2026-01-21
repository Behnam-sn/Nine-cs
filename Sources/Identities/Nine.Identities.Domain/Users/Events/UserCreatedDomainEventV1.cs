using Nine.Identities.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Users.Events;

public sealed record UserCreatedDomainEventV1(
    DomainEventId Id,
    UserId UserId,
    Name Name,
    Email Email,
    PhoneNumber PhoneNumber,
    Username Username,
    DateTime OccurredAt
) : IDomainEvent;