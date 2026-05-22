using Nine.Identities.Domain.Accounts.Enums;
using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Events;

public sealed record CredentialAddedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    CredentialId CredentialId,
    CredentialType CredentialType,
    HashedSecret HashedSecret,
    DateTime Timestamp
) : IDomainEvent;
