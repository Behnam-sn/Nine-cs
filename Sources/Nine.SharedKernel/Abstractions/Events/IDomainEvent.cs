using MediatR;

using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.SharedKernel.Abstractions.Events;

public interface IDomainEvent : INotification
{
    DomainEventId Id { get; }

    DateTime Timestamp { get; }
}
