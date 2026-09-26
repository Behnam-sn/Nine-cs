using Nine.Shared.Domain.Abstractions.Entities;
using Nine.Shared.Domain.Abstractions.Events;

namespace Nine.Shared.Domain.Abstractions.AggregateRoots;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];
    
    protected AggregateRoot(
        TId id,
        bool isDeleted,
        DateTime createdAtUtc,
        DateTime? modifiedAtUtc = null
    ) : base(id, isDeleted, createdAtUtc, modifiedAtUtc)
    {
    }

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
