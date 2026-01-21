using Nine.SharedKernel.Abstractions.Events;

namespace Nine.SharedKernel.Abstractions.AggregateRoots;

public interface IAggregateRoot
{
    IEnumerable<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}