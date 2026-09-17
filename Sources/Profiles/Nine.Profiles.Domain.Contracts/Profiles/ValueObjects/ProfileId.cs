using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;

namespace Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

public readonly record struct ProfileId
{
    private ProfileId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public override string ToString()
    {
        return Value.ToString();
    }
    
    public static ProfileId Create()
    {
        return new(Guid.NewGuid());
    }
    
    public static ProfileId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ProfileIdCannotBeEmptyException();
        }

        return new(value);
    }

    public static ProfileId Parse(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new ProfileIdInvalidFormatException();
        }

        return From(guid);
    }
}