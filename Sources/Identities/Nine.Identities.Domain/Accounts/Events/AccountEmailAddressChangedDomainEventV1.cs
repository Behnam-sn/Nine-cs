using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Events;

public sealed record AccountEmailAddressChangedDomainEventV1(DomainEventId Id, AccountId AccountId, EmailAddress EmailAddress, DateTime OccurredAt)
    : IDomainEvent;