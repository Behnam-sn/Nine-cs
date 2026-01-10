using Nine.Domain.Abstractions.Events;
using Nine.Domain.Abstractions.ValueObjects;

namespace Nine.Domain.Users.Events;

public sealed record UserFirstNameChangedDomainEventV1 : IDomainEvent
{
    private UserFirstNameChangedDomainEventV1(Guid Id, Guid UserId, string FirstName, DateTime OccurredAt)
    {
        this.Id = Id;
        this.UserId = UserId;
        this.FirstName = FirstName;
        this.OccurredAt = OccurredAt;
    }


    public Guid Id { get; }

    public Guid UserId { get; }

    public string FirstName { get; init; }

    public DateTime OccurredAt { get; init; }


    public static UserFirstNameChangedDomainEventV1 Create(Guid userId, string firstName)
    {
        return new(
            Id: DomainEventId.Create().Value,
            UserId: userId,
            FirstName: firstName,
            OccurredAt: DateTime.UtcNow
        );
    }
}