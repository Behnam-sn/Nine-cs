using Nine.Domain.Abstractions.ValueObjects;

namespace Nine.Domain.Abstractions.Entities;

public interface IDomainEvent
{
    public DomainEventId Id { get; }
}