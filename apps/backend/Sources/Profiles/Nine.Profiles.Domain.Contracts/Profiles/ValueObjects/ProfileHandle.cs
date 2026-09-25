using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;

namespace Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

public readonly record struct ProfileHandle
{
    public const int MinLength = 3;
    public const int MaxLength = 20;
    
    private ProfileHandle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ProfileHandle Create(string value)
    {
        if (value.Length < MinLength)
        {
            throw new ProfileHandleTooShortException();
        }
        
        if (value.Length > MaxLength)
        {
            throw new ProfileHandleTooLongException();
        }
        
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ProfileHandleInvalidCharactersException();
        }

        if (value.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
        {
            throw new ProfileHandleInvalidCharactersException();
        }

        value = value.ToLower();
        return new(value);
    }
}
