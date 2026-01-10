using Nine.Domain.Abstractions.Events;

namespace Nine.Domain.Abstractions.AggregateRoots;

public interface IAggregateRoot
{
    IEnumerable<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}