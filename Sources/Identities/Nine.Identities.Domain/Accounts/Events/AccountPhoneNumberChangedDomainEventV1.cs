using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Events;

public sealed record AccountPhoneNumberChangedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    PhoneNumber PhoneNumber,
    DateTime Timestamp
) : IDomainEvent;