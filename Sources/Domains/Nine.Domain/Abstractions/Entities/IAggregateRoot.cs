namespace Nine.Domain.Abstractions.Entities;

public interface IAggregateRoot
{
    IEnumerable<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}