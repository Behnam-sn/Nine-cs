using Nine.Domain.Abstractions.Entities;
using Nine.Domain.Abstractions.ValueObjects;

namespace Nine.Domain.Abstractions.Events;

public abstract record DomainEvent(DomainEventId Id) : IDomainEvent;