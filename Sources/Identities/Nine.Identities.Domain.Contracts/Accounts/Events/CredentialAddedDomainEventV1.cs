using Nine.Identities.Domain.Contracts.Accounts.Enums;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Accounts.Events;

public sealed record CredentialAddedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    CredentialId CredentialId,
    CredentialType CredentialType,
    HashedSecret HashedSecret,
    DateTime Timestamp
) : IDomainEvent;
