using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Events;

public sealed record AccountNameChangedDomainEventV1(DomainEventId Id, AccountId AccountId, Name Name, DateTime OccurredAt)
    : IDomainEvent;