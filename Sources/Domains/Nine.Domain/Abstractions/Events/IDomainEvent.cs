namespace Nine.Domain.Abstractions.Events;

public interface IDomainEvent
{
    public Guid Id { get; }
    
    DateTime OccurredAt { get; }
}