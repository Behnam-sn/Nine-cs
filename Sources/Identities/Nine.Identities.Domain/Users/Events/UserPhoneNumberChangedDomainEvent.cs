using Nine.Identities.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Users.Events;

public sealed record UserPhoneNumberChangedDomainEvent(
    DomainEventId Id,
    UserId UserId,
    PhoneNumber PhoneNumber,
    DateTime OccurredAt
) : IDomainEvent;