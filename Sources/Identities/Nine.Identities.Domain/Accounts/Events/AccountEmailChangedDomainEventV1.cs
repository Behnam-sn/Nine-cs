using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Events;

public sealed record AccountEmailChangedDomainEventV1(DomainEventId Id, AccountId AccountId, Email Email, DateTime OccurredAt)
    : IDomainEvent;