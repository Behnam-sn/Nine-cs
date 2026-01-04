using Nine.Domain.Abstractions.Entities;
using Nine.Domain.Abstractions.Events;

namespace Nine.Domain.Abstractions.AggregateRoots;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;

    protected AggregateRoot(
        TId id,
        bool isDeleted,
        DateTime createdAtUtc,
        DateTime? modifiedAtUtc = null
    ) : base(id, isDeleted, createdAtUtc, modifiedAtUtc)
    {
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}