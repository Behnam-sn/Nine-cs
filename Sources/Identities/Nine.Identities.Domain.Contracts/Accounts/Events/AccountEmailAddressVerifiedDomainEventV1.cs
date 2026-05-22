using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Accounts.Events;

public sealed record AccountEmailAddressVerifiedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    EmailAddress EmailAddress,
    DateTime Timestamp
) : IDomainEvent;
