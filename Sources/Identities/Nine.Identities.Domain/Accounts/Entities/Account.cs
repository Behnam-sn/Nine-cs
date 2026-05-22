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
    public EmailAddress EmailAddress { get; private set; }
    public bool IsEmailAddressVerified { get; private set; }
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

    public void SetEmailAddress(EmailAddress emailAddress)
    {
        var accountEmailAddressChangedDomainEvent = new AccountEmailAddressChangedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            EmailAddress: emailAddress,
            Timestamp: DateTime.UtcNow
        );
        RaiseDomainEvent(accountEmailAddressChangedDomainEvent);
        ApplyDomainEvent(accountEmailAddressChangedDomainEvent);
    }

    public void VerifyEmailAddress()
    {
        if (IsEmailAddressVerified)
        {
            return;
        }

        var accountEmailAddressVerifiedDomainEvent = new AccountEmailAddressVerifiedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            EmailAddress: EmailAddress,
            Timestamp: DateTime.UtcNow
        );
        RaiseDomainEvent(accountEmailAddressVerifiedDomainEvent);
        ApplyDomainEvent(accountEmailAddressVerifiedDomainEvent);
    }

    public void SetPhoneNumber(PhoneNumber phoneNumber)
    {
        var accountPhoneNumberChangedDomainEvent = new AccountPhoneNumberChangedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            PhoneNumber: phoneNumber,
            Timestamp: DateTime.UtcNow
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
            Timestamp: DateTime.UtcNow
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
            Timestamp: DateTime.UtcNow
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
            Timestamp: DateTime.UtcNow
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
            Timestamp: DateTime.UtcNow
        );
        RaiseDomainEvent(credentialChangedDomainEvent);
        ApplyDomainEvent(credentialChangedDomainEvent);
    }

    private void ApplyDomainEvent(AccountCreatedDomainEventV1 domainEvent)
    {
        AccountId = domainEvent.AccountId;
        EmailAddress = domainEvent.EmailAddress;
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

    private void ApplyDomainEvent(AccountEmailAddressChangedDomainEventV1 domainEvent)
    {
        EmailAddress = domainEvent.EmailAddress;
        IsEmailAddressVerified = false;
    }

    private void ApplyDomainEvent(AccountEmailAddressVerifiedDomainEventV1 domainEvent)
    {
        IsEmailAddressVerified = true;
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

    public static Account CreateInstance(EmailAddress emailAddress, CredentialId credentialId, CredentialType credentialType, HashedSecret hashedSecret, PhoneNumber? phoneNumber = null)
    {
        var account = new Account();
        var accountCreatedDomainEvent = new AccountCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId.Create(),
            EmailAddress: emailAddress,
            PhoneNumber: phoneNumber,
            InitialCredentialId: credentialId,
            InitialCredentialType: credentialType,
            InitialHashedSecret: hashedSecret,
            Timestamp: DateTime.UtcNow
        );
        account.RaiseDomainEvent(accountCreatedDomainEvent);
        account.ApplyDomainEvent(accountCreatedDomainEvent);
        return account;
    }
}