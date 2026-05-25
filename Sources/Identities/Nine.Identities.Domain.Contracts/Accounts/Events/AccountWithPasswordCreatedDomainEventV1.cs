using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Accounts.Events;

public sealed record AccountWithPasswordCreatedDomainEventV1(
    DomainEventId Id,
    AccountId AccountId,
    EmailAddress EmailAddress,
    PhoneNumber? PhoneNumber,
    CredentialId CredentialId,
    HashedSecret HashedPassword,
    DateTime Timestamp
) : IDomainEvent;
