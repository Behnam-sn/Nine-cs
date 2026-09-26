using MediatR;

using Nine.Shared.Domain.Abstractions.ValueObjects;

namespace Nine.Shared.Domain.Abstractions.Events;

public interface IDomainEvent : INotification
{
    DomainEventId Id { get; }

    DateTime Timestamp { get; }
}
