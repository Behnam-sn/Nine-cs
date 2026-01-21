namespace Nine.SharedKernel.Abstractions.Entities;

public abstract class Entity<TId> : IEntity
{
    public TId Id { get; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime? ModifiedAtUtc { get; protected set; }

    protected Entity(
        TId id,
        bool isDeleted,
        DateTime createdAtUtc,
        DateTime? modifiedAtUtc = null
    )
    {
        Id = id;
        IsDeleted = isDeleted;
        CreatedAtUtc = createdAtUtc;
        ModifiedAtUtc = modifiedAtUtc;
    }
}