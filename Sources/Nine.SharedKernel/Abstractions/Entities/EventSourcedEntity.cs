namespace Nine.SharedKernel.Abstractions.Entities;

public abstract class EventSourcedEntity<TId> : IEntity
{
    public TId Id { get; protected set; }
}