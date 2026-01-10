using Nine.Domain.Abstractions.AggregateRoots;
using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;
using Nine.Domain.Users.Events;

namespace Nine.Domain.Users.Entities;

public sealed class User : EventSourcedAggregateRoot
{
    public User()
    {
    }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    private void Apply(UserCreatedDomainEventV1 domainEvent)
    {
        FirstName = domainEvent.FirstName;
        LastName = domainEvent.LastName;
    }

    public static User Create(string firstName, string lastName)
    {
        var user = new User();
        var userCreatedEvent = new UserCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            FirstName: firstName,
            LastName: lastName,
            OccurredAt: DateTime.UtcNow
        );
        user.RaiseDomainEvent(userCreatedEvent);
        return user;
    }
}