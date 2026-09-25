using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;

namespace Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

public readonly record struct ProfileName
{
    public const int MaxLength = 100;
    
    private ProfileName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ProfileName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ProfileNameCannotBeEmptyException();
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            throw new ProfileNameTooLongException();
        }

        return new(value);
    }
}