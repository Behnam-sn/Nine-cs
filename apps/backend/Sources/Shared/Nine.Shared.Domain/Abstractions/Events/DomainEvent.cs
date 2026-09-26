using Nine.Shared.Domain.Abstractions.ValueObjects;

namespace Nine.Shared.Domain.Abstractions.Events;

public abstract record DomainEvent(DomainEventId Id, DateTime Timestamp) : IDomainEvent;
