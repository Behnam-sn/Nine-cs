using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;

namespace Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

public readonly record struct ProfileAvatar
{
    private static readonly string[] SupportedMediaTypes =
    [
        "image/gif",
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private ProfileAvatar(string objectKey, string mediaType)
    {
        ObjectKey = objectKey;
        MediaType = mediaType;
    }

    public string ObjectKey { get; }
    public string MediaType { get; }

    public static ProfileAvatar Create(string objectKey, string mediaType)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ProfileAvatarObjectKeyCannotBeEmptyException();
        }

        if (string.IsNullOrWhiteSpace(mediaType))
        {
            throw new ProfileAvatarInvalidMediaTypeException();
        }

        objectKey = objectKey.Trim();
        mediaType = mediaType.Trim().ToLowerInvariant();

        if (!SupportedMediaTypes.Contains(mediaType, StringComparer.Ordinal))
        {
            throw new ProfileAvatarInvalidMediaTypeException();
        }

        return new(objectKey, mediaType);
    }
}
