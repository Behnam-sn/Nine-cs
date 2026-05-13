using Nine.SharedKernel.Abstractions.Entities;
using Nine.SharedKernel.Abstractions.Events;

namespace Nine.SharedKernel.Abstractions.AggregateRoots;

public abstract class EventSourcedAggregateRoot<TId> : EventSourcedEntity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    protected void ApplyDomainEvent(IDomainEvent domainEvent)
    {
        var applyMethod = this.GetType()
            .GetMethod("Apply", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, 
                null, [domainEvent.GetType()], null);
    
        applyMethod?.Invoke(this, [domainEvent]);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}