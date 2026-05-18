using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Events;

public sealed record AccountCreatedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    Name Name,
    Email Email,
    PhoneNumber PhoneNumber,
    DateTime OccurredAt
) : IDomainEvent;