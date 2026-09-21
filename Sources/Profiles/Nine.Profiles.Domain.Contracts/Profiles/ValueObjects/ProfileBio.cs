using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;

namespace Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

public readonly record struct ProfileBio
{
    public const int MaxLength = 500;

    private ProfileBio(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ProfileBio Create(string? value)
    {
        value ??= string.Empty;
        value = value.Trim();

        if (value.Length > MaxLength)
        {
            throw new ProfileBioTooLongException();
        }

        return new(value);
    }
}
