namespace Nine.Profiles.Presentation.Profiles.WebApi.Requests;

public record CreateProfileRequestV1(
    string Name,
    string Handle,
    string? AvatarObjectKey,
    string? AvatarMediaType,
    string Bio
);
