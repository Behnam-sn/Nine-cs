using Nine.Shared.Domain.Abstractions.Entities;
using Nine.Shared.Domain.Abstractions.Events;

namespace Nine.Shared.Domain.Abstractions.AggregateRoots;

public abstract class EventSourcedAggregateRoot<TId> : EventSourcedEntity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;

    protected void ApplyDomainEvent(IDomainEvent domainEvent)
    {
        var applyDomainEventMethod = this.GetType()
            .GetMethod("ApplyDomainEvent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, [domainEvent.GetType()], null);

        applyDomainEventMethod?.Invoke(this, [domainEvent]);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
