using Nine.SharedKernel.Abstractions.Entities;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.SharedKernel.Abstractions.Events;

public abstract record DomainEvent(DomainEventId Id, DateTime OccurredAt) : IDomainEvent;