using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Accounts.Events;

public sealed record AccountPasswordCredentialChangedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    CredentialId CredentialId,
    HashedSecret NewHashedPassword,
    DateTime Timestamp
) : IDomainEvent;
