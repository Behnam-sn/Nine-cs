using Nine.Shared.Domain.Abstractions.Events;

namespace Nine.Shared.Domain.Abstractions.AggregateRoots;

public interface IAggregateRoot
{
    IEnumerable<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
