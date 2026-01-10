using Nine.Domain.Abstractions.Entities;
using Nine.Domain.Abstractions.Events;

namespace Nine.Domain.Abstractions.AggregateRoots;

public abstract class EventSourcedAggregateRoot<TId> : EventSourcedEntity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}