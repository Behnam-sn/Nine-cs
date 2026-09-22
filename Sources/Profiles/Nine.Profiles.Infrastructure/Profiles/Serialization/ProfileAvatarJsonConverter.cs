using System.Text.Json;
using System.Text.Json.Serialization;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Infrastructure.Profiles.Serialization;

public sealed class ProfileAvatarJsonConverter : JsonConverter<ProfileAvatar>
{
    public override ProfileAvatar Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Cannot deserialize {nameof(ProfileAvatar)} from {reader.TokenType}.");
        }

        string? objectKey = null;
        string? mediaType = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Cannot deserialize {nameof(ProfileAvatar)}.");
            }

            var propertyName = reader.GetString();
            if (!reader.Read())
            {
                throw new JsonException($"Cannot deserialize {nameof(ProfileAvatar)}.");
            }

            if (Matches(propertyName, nameof(ProfileAvatar.ObjectKey), options))
            {
                objectKey = ReadString(ref reader);
            }
            else if (Matches(propertyName, nameof(ProfileAvatar.MediaType), options))
            {
                mediaType = ReadString(ref reader);
            }
            else
            {
                reader.Skip();
            }
        }

        if (objectKey is null || mediaType is null)
        {
            throw new JsonException($"Cannot deserialize {nameof(ProfileAvatar)}.");
        }

        return ProfileAvatar.Create(objectKey, mediaType);
    }

    public override void Write(Utf8JsonWriter writer, ProfileAvatar value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(Name(nameof(ProfileAvatar.ObjectKey), options), value.ObjectKey);
        writer.WriteString(Name(nameof(ProfileAvatar.MediaType), options), value.MediaType);
        writer.WriteEndObject();
    }

    private static string? ReadString(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Cannot deserialize {nameof(ProfileAvatar)}.");
        }

        return reader.GetString();
    }

    private static bool Matches(string? actual, string propertyName, JsonSerializerOptions options)
    {
        if (actual is null)
        {
            return false;
        }

        var comparison = options.PropertyNameCaseInsensitive
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return actual.Equals(propertyName, comparison) || actual.Equals(Name(propertyName, options), comparison);
    }

    private static string Name(string propertyName, JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
    }
}
