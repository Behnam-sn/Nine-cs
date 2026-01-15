using Nine.Domain.Abstractions.AggregateRoots;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.Enums;
using Nine.Domain.Users.Events;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Entities;

public sealed class User : EventSourcedAggregateRoot<UserId>
{
    public UserId UserId { get; private set; }
    public Name Name { get; set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Username Username { get; private set; }
    public UserStates State { get; private set; }

    private User()
    {
    }

    public User(Name name, Email email, PhoneNumber phoneNumber, Username username)
    {
        var userCreatedEvent = new UserCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId.Create(),
            Name: name,
            Email: email,
            PhoneNumber: phoneNumber,
            Username: username,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userCreatedEvent);
    }

    public void ChangeName(Name name)
    {
        var userFirstNameChangedDomainEvent = new UserNameChangedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId,
            Name: Name,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userFirstNameChangedDomainEvent);
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

    public void SetUsername(Username username)
    {
        var userUsernameChangedDomainEvent = new UserUsernameChangedDomainEvent(
            Id: DomainEventId.Create(),
            UserId: UserId,
            Username: username,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userUsernameChangedDomainEvent);
    }

    private void Activate()
    {
        throw new NotImplementedException();
    }

    private void Archive()
    {
        throw new NotImplementedException();
    }

    private void Suspend()
    {
        throw new NotImplementedException();
    }

    private void Delete()
    {
        throw new NotImplementedException();
    }

    private void Apply(UserCreatedDomainEventV1 domainEvent)
    {
        UserId = domainEvent.UserId;
        Name = domainEvent.Name;
        Email = domainEvent.Email;
        PhoneNumber = domainEvent.PhoneNumber;
        Username = domainEvent.Username;
        State = UserStates.Active;
    }

    private void Apply(UserNameChangedDomainEventV1 domainEvent)
    {
        Name = domainEvent.Name;
    }

    private void Apply(UserEmailChangedDomainEventV1 domainEvent)
    {
        Email = domainEvent.Email;
    }

    private void Apply(UserPhoneNumberChangedDomainEvent domainEvent)
    {
        PhoneNumber = domainEvent.PhoneNumber;
    }

    private void Apply(UserUsernameChangedDomainEvent domainEvent)
    {
        Username = domainEvent.Username;
    }
}