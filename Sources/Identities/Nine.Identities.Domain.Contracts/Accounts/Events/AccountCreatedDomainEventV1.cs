using Nine.Identities.Domain.Contracts.Accounts.Enums;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Accounts.Events;

public sealed record AccountCreatedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    EmailAddress EmailAddress,
    PhoneNumber? PhoneNumber,
    CredentialId InitialCredentialId,
    CredentialType InitialCredentialType,
    HashedSecret InitialHashedSecret,
    DateTime Timestamp
) : IDomainEvent;
