using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.SharedKernel.Abstractions.Events;

public interface IDomainEvent
{
    DomainEventId Id { get; }

    DateTime OccurredAt { get; }
}