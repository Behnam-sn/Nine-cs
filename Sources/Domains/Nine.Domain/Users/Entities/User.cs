using Nine.Domain.Abstractions.AggregateRoots;

namespace Nine.Domain.Users.Entities;

public sealed class User : EventSourcedAggregateRoot
{
    private User(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }
}