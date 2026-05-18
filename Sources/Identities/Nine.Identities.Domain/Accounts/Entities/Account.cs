using Nine.Identities.Domain.Accounts.Events;
using Nine.Identities.Domain.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.AggregateRoots;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Accounts.Entities;

public sealed class Account : EventSourcedAggregateRoot<AccountId>
{
    public AccountId AccountId { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }

    public Account()
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

    private void ApplyDomainEvent(AccountCreatedDomainEventV1 domainEvent)
    {
        AccountId = domainEvent.AccountId;
        Email = domainEvent.Email;
        PhoneNumber = domainEvent.PhoneNumber;
    }

    private void ApplyDomainEvent(AccountEmailChangedDomainEventV1 domainEvent)
    {
        Email = domainEvent.Email;
    }

    private void ApplyDomainEvent(AccountPhoneNumberChangedDomainEventV1 domainEvent)
    {
        PhoneNumber = domainEvent.PhoneNumber;
    }

    public static Account CreateInstance(Email email, PhoneNumber phoneNumber)
    {
        var account = new Account();
        var accountCreatedDomainEvent = new AccountCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            AccountId: AccountId.Create(),
            Email: email,
            PhoneNumber: phoneNumber,
            OccurredAt: DateTime.UtcNow
        );
        account.RaiseDomainEvent(accountCreatedDomainEvent);
        account.ApplyDomainEvent(accountCreatedDomainEvent);
        return account;
    }
}