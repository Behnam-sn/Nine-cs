using Nine.Domain.Abstractions.AggregateRoots;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.Events;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Entities;

public sealed class User : EventSourcedAggregateRoot<UserId>
{
    public UserId UserId { get; private set; }
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }

    private User()
    {
    }

    public User(FirstName firstName, LastName lastName, Email email, PhoneNumber phoneNumber)
    {
        var userCreatedEvent = new UserCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId.Create(),
            FirstName: firstName,
            LastName: lastName,
            Email: email,
            PhoneNumber: phoneNumber,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userCreatedEvent);
    }

    public void ChangeFirstName(FirstName firstName)
    {
        var userFirstNameChangedDomainEvent = new UserFirstNameChangedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId,
            FirstName: firstName,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userFirstNameChangedDomainEvent);
    }

    public void ChangeLastName(LastName lastName)
    {
        var userLastNameChangedDomainEvent = new UserLastNameChangedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId,
            LastName: lastName,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userLastNameChangedDomainEvent);
    }

    public void SetEmail(Email email)
    {
        var userEmailChangedDomainEvent = new UserEmailChangedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId,
            Email: email,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userEmailChangedDomainEvent);
    }

    public void SetPhoneNumber(PhoneNumber phoneNumber)
    {
        var userPhoneNumberChangedDomainEvent = new UserPhoneNumberChangedDomainEvent(
            Id: DomainEventId.Create(),
            UserId: UserId,
            PhoneNumber: phoneNumber,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userPhoneNumberChangedDomainEvent);
    }

    private void Apply(UserCreatedDomainEventV1 domainEvent)
    {
        UserId = domainEvent.UserId;
        FirstName = domainEvent.FirstName;
        LastName = domainEvent.LastName;
        Email = domainEvent.Email;
        PhoneNumber = domainEvent.PhoneNumber;
    }

    private void Apply(UserFirstNameChangedDomainEventV1 domainEvent)
    {
        FirstName = domainEvent.FirstName;
    }

    private void Apply(UserLastNameChangedDomainEventV1 domainEvent)
    {
        LastName = domainEvent.LastName;
    }

    private void Apply(UserEmailChangedDomainEventV1 domainEvent)
    {
        Email = domainEvent.Email;
    }

    private void Apply(UserPhoneNumberChangedDomainEvent domainEvent)
    {
        PhoneNumber = domainEvent.PhoneNumber;
    }
}