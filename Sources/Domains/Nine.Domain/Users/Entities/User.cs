using Nine.Domain.Abstractions.AggregateRoots;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.Events;
using Nine.Domain.Users.ValueObjects;

namespace Nine.Domain.Users.Entities;

public sealed class User : EventSourcedAggregateRoot<UserId>
{
    public User()
    {
    }

    public UserId UserId { get; private set; }

    public FirstName FirstName { get; private set; }

    public string LastName { get; private set; }

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

    private void Apply(UserCreatedDomainEventV1 domainEvent)
    {
        UserId = domainEvent.UserId;
        FirstName = domainEvent.FirstName;
        LastName = domainEvent.LastName;
    }

    private void Apply(UserFirstNameChangedDomainEventV1 domainEvent)
    {
        FirstName = domainEvent.FirstName;
    }

    public static User Create(FirstName firstName, string lastName)
    {
        var user = new User();
        var userCreatedEvent = new UserCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: UserId.Create(),
            FirstName: firstName,
            LastName: lastName,
            OccurredAt: DateTime.UtcNow
        );
        user.RaiseDomainEvent(userCreatedEvent);
        return user;
    }
}