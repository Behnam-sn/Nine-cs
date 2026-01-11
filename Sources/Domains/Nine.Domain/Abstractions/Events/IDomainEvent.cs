using Nine.Domain.Abstractions.ValueObjects;

namespace Nine.Domain.Abstractions.Events;

public interface IDomainEvent
{
    DomainEventId Id { get; }
    
    DateTime OccurredAt { get; }
}