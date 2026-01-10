using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserFirstNameChangedDomainEventV1 : IDomainEvent
{
    private UserFirstNameChangedDomainEventV1(DomainEventId Id, string FirstName, DateTime OccurredAt)
    {
        this.Id = Id;
        this.FirstName = FirstName;
        this.OccurredAt = OccurredAt;
    }


    public DomainEventId Id { get; init; }

    public string FirstName { get; init; }

    public DateTime OccurredAt { get; init; }


    public static UserFirstNameChangedDomainEventV1 Create(string firstName)
    {
        return new(
            Id: DomainEventId.Create(),
            FirstName: firstName,
            OccurredAt: DateTime.UtcNow
        );
    }
}