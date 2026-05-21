using Nine.Identities.Domain.Accounts.Enums;
using Nine.Identities.Domain.Accounts.Events;
using Nine.Identities.Domain.Accounts.Exceptions;
using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.AggregateRoots;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Entities;

public sealed class Account : EventSourcedAggregateRoot<AccountId>
{
    private List<Credential> _credentials = [];

    public AccountId AccountId { get; private set; }
    public Email Email { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public bool IsPhoneNumberVerified { get; private set; }
    public IEnumerable<Credential> Credentials => _credentials;

    private Account()
    {
    }

    public Account(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            ApplyDomainEvent(domainEvent);
        }
    }

    public void SetEmail(Email email)
    {
        var accountEmailChangedDomainEvent = new AccountEmailChangedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            Email: email,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(accountEmailChangedDomainEvent);
        ApplyDomainEvent(accountEmailChangedDomainEvent);
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
        {
            return;
        }

        var accountEmailVerifiedDomainEvent = new AccountEmailVerifiedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            Email: Email,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(accountEmailVerifiedDomainEvent);
        ApplyDomainEvent(accountEmailVerifiedDomainEvent);
    }

    public void SetPhoneNumber(PhoneNumber phoneNumber)
    {
        var accountPhoneNumberChangedDomainEvent = new AccountPhoneNumberChangedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            PhoneNumber: phoneNumber,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(accountPhoneNumberChangedDomainEvent);
        ApplyDomainEvent(accountPhoneNumberChangedDomainEvent);
    }

    public void VerifyPhoneNumber()
    {
        if (!PhoneNumber.HasValue)
        {
            throw new AccountPhoneNumberNotSetException();
        }

        if (IsPhoneNumberVerified)
        {
            return;
        }

        var accountPhoneNumberVerifiedDomainEvent = new AccountPhoneNumberVerifiedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            PhoneNumber: PhoneNumber.Value,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(accountPhoneNumberVerifiedDomainEvent);
        ApplyDomainEvent(accountPhoneNumberVerifiedDomainEvent);
    }

    public void AddCredential(CredentialId credentialId, CredentialType type, HashedSecret secret)
    {
        if (_credentials.Any(i => i.Id == credentialId) || _credentials.Any(i => i.Type == type))
        {
            throw new CredentialAlreadyExistsException();
        }

        var credentialAddedDomainEvent = new CredentialAddedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            CredentialId: credentialId,
            CredentialType: type,
            HashedSecret: secret,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(credentialAddedDomainEvent);
        ApplyDomainEvent(credentialAddedDomainEvent);
    }

    public void RemoveCredential(CredentialId credentialId)
    {
        var credential = _credentials.FirstOrDefault(i => i.Id == credentialId);
        if (credential == null)
        {
            throw new CredentialNotFoundException();
        }

        if (_credentials.Count <= 1)
        {
            throw new CannotRemoveLastCredentialException();
        }

        var credentialRemovedDomainEvent = new CredentialRemovedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            CredentialId: credentialId,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(credentialRemovedDomainEvent);
        ApplyDomainEvent(credentialRemovedDomainEvent);
    }

    public void ChangeCredential(CredentialId credentialId, HashedSecret newSecret)
    {
        var credential = _credentials.FirstOrDefault(i => i.Id == credentialId);
        if (credential == null)
        {
            throw new CredentialNotFoundException();
        }

        var credentialChangedDomainEvent = new CredentialChangedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            CredentialId: credentialId,
            NewHashedSecret: newSecret,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(credentialChangedDomainEvent);
        ApplyDomainEvent(credentialChangedDomainEvent);
    }

    private void ApplyDomainEvent(AccountCreatedDomainEventV1 domainEvent)
    {
        AccountId = domainEvent.AccountId;
        Email = domainEvent.Email;
        PhoneNumber = domainEvent.PhoneNumber;
        _credentials =
        [
            Credential.CreateInstance(
                id: domainEvent.InitialCredentialId,
                type: domainEvent.InitialCredentialType,
                secret: domainEvent.InitialHashedSecret
            )
        ];
    }

    private void ApplyDomainEvent(AccountEmailChangedDomainEventV1 domainEvent)
    {
        Email = domainEvent.Email;
        IsEmailVerified = false;
    }

    private void ApplyDomainEvent(AccountEmailVerifiedDomainEventV1 domainEvent)
    {
        IsEmailVerified = true;
    }

    private void ApplyDomainEvent(AccountPhoneNumberChangedDomainEventV1 domainEvent)
    {
        PhoneNumber = domainEvent.PhoneNumber;
        IsPhoneNumberVerified = false;
    }

    private void ApplyDomainEvent(AccountPhoneNumberVerifiedDomainEventV1 domainEvent)
    {
        IsPhoneNumberVerified = true;
    }

    private void ApplyDomainEvent(CredentialAddedDomainEventV1 domainEvent)
    {
        var credential = Credential.CreateInstance(
            id: domainEvent.CredentialId,
            type: domainEvent.CredentialType,
            secret: domainEvent.HashedSecret
        );
        _credentials.Add(credential);
    }

    private void ApplyDomainEvent(CredentialRemovedDomainEventV1 domainEvent)
    {
        _credentials.RemoveAll(i => i.Id == domainEvent.CredentialId);
    }

    private void ApplyDomainEvent(CredentialChangedDomainEventV1 domainEvent)
    {
        var credential = _credentials.First(i => i.Id == domainEvent.CredentialId);
        credential.SetSecret(domainEvent.NewHashedSecret);
    }

    public static Account CreateInstance(Email email, CredentialId credentialId, CredentialType credentialType, HashedSecret hashedSecret, PhoneNumber? phoneNumber = null)
    {
        var account = new Account();
        var accountCreatedDomainEvent = new AccountCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId.Create(),
            Email: email,
            PhoneNumber: phoneNumber,
            InitialCredentialId: credentialId,
            InitialCredentialType: credentialType,
            InitialHashedSecret: hashedSecret,
            OccurredAt: DateTime.UtcNow
        );
        account.RaiseDomainEvent(accountCreatedDomainEvent);
        account.ApplyDomainEvent(accountCreatedDomainEvent);
        return account;
    }
}