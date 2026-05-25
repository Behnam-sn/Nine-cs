using Nine.Identities.Domain.Contracts.Accounts.Enums;
using Nine.Identities.Domain.Contracts.Accounts.Events;
using Nine.Identities.Domain.Contracts.Accounts.Exceptions;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.AggregateRoots;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Entities;

public sealed class Account : EventSourcedAggregateRoot<AccountId>
{
    private readonly List<Credential> _credentials = [];

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

    public AccountId AccountId { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public bool IsEmailAddressVerified { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public bool IsPhoneNumberVerified { get; private set; }
    public IEnumerable<Credential> Credentials => _credentials;

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

    public void AddPasswordCredential(HashedSecret hashedPassword)
    {
        if (_credentials.Any(i => i.Type == CredentialType.Password))
        {
            throw new CredentialAlreadyExistsException();
        }

        var accountPasswordCredentialAddedDomainEvent = new AccountPasswordCredentialAddedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            CredentialId: CredentialId.Create(),
            HashedPassword: hashedPassword,
            Timestamp: DateTime.UtcNow
        );
        RaiseDomainEvent(accountPasswordCredentialAddedDomainEvent);
        ApplyDomainEvent(accountPasswordCredentialAddedDomainEvent);
    }

    public void ChangePasswordCredential(CredentialId credentialId, HashedSecret newHashedPassword)
    {
        var credential = _credentials.FirstOrDefault(i => i.Id == credentialId);
        if (credential == null)
        {
            throw new CredentialNotFoundException();
        }

        var accountPasswordCredentialChangedDomainEvent = new AccountPasswordCredentialChangedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            CredentialId: credentialId,
            NewHashedPassword: newHashedPassword,
            Timestamp: DateTime.UtcNow
        );
        RaiseDomainEvent(accountPasswordCredentialChangedDomainEvent);
        ApplyDomainEvent(accountPasswordCredentialChangedDomainEvent);
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

        var credentialRemovedDomainEvent = new AccountCredentialRemovedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId,
            CredentialId: credentialId,
            Timestamp: DateTime.UtcNow
        );
        RaiseDomainEvent(credentialRemovedDomainEvent);
        ApplyDomainEvent(credentialRemovedDomainEvent);
    }

    private void ApplyDomainEvent(AccountWithPasswordCreatedDomainEventV1 domainEvent)
    {
        AccountId = domainEvent.AccountId;
        EmailAddress = domainEvent.EmailAddress;
        PhoneNumber = domainEvent.PhoneNumber;
        _credentials.Add(
            new PasswordCredential(domainEvent.CredentialId, domainEvent.HashedPassword)
        );
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

    private void ApplyDomainEvent(AccountPasswordCredentialAddedDomainEventV1 domainEvent)
    {
        var credential = new PasswordCredential(domainEvent.CredentialId, domainEvent.HashedPassword);
        _credentials.Add(credential);
    }

    private void ApplyDomainEvent(AccountPasswordCredentialChangedDomainEventV1 domainEvent)
    {
        var credential = (PasswordCredential)_credentials.First(i => i.Id == domainEvent.CredentialId);
        credential.SetHashedPassword(domainEvent.NewHashedPassword);
    }

    private void ApplyDomainEvent(AccountCredentialRemovedDomainEventV1 domainEvent)
    {
        _credentials.RemoveAll(i => i.Id == domainEvent.CredentialId);
    }

    public static Account CreateWithPassword(EmailAddress emailAddress, HashedSecret hashedPassword, PhoneNumber? phoneNumber = null)
    {
        var account = new Account();
        var accountCreatedDomainEvent = new AccountWithPasswordCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId.Create(),
            EmailAddress: emailAddress,
            PhoneNumber: phoneNumber,
            CredentialId: CredentialId.Create(),
            HashedPassword: hashedPassword,
            Timestamp: DateTime.UtcNow
        );
        account.RaiseDomainEvent(accountCreatedDomainEvent);
        account.ApplyDomainEvent(accountCreatedDomainEvent);
        return account;
    }
}
