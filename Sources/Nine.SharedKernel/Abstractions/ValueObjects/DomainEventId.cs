namespace Nine.SharedKernel.Abstractions.ValueObjects;

public readonly record struct DomainEventId
{
    public Guid Value { get; }

    private DomainEventId(Guid value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static DomainEventId Create()
    {
        return new(Guid.NewGuid());
    }

    public static DomainEventId Parse(Guid value)
    {
        return new(value);
    }
}