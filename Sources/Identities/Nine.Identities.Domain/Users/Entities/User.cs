using Nine.Identities.Domain.Users.Enums;
using Nine.Identities.Domain.Users.Events;
using Nine.Identities.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.AggregateRoots;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Users.Entities;

public sealed class User : EventSourcedAggregateRoot<UserId>
{
    public UserId UserId { get; private set; }
    public Name Name { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public UserStates State { get; private set; }

    public User()
    {
        
    }

    public User(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)  
        {
            ApplyDomainEvent(domainEvent);
        }
    }

    public void SetName(Name name)
    {
        var userNameChangedDomainEvent = new UserNameChangedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId,
            Name: name,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userNameChangedDomainEvent);
        ApplyDomainEvent(userNameChangedDomainEvent);
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
        ApplyDomainEvent(userEmailChangedDomainEvent);
    }

    public void SetPhoneNumber(PhoneNumber phoneNumber)
    {
        var userPhoneNumberChangedDomainEvent = new UserPhoneNumberChangedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId,
            PhoneNumber: phoneNumber,
            OccurredAt: DateTime.UtcNow
        );
        RaiseDomainEvent(userPhoneNumberChangedDomainEvent);
        ApplyDomainEvent(userPhoneNumberChangedDomainEvent);
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

    private void ApplyDomainEvent(UserCreatedDomainEventV1 domainEvent)
    {
        UserId = domainEvent.UserId;
        Name = domainEvent.Name;
        Email = domainEvent.Email;
        PhoneNumber = domainEvent.PhoneNumber;
        State = UserStates.Active;
    }

    private void ApplyDomainEvent(UserNameChangedDomainEventV1 domainEvent)
    {
        Name = domainEvent.Name;
    }

    private void ApplyDomainEvent(UserEmailChangedDomainEventV1 domainEvent)
    {
        Email = domainEvent.Email;
    }

    private void ApplyDomainEvent(UserPhoneNumberChangedDomainEventV1 domainEvent)
    {
        PhoneNumber = domainEvent.PhoneNumber;
    }

    public static User CreateInstance(Name name, Email email, PhoneNumber phoneNumber)
    {
        var user = new User();
        var userCreatedDomainEvent = new UserCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId.Create(),
            Name: name,
            Email: email,
            PhoneNumber: phoneNumber,
            OccurredAt: DateTime.UtcNow
        );
        user.RaiseDomainEvent(userCreatedDomainEvent);
        user.ApplyDomainEvent(userCreatedDomainEvent);
        return user;
    }
}