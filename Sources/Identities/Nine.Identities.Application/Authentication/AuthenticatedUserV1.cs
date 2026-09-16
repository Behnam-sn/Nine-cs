namespace Nine.Identities.Application.Authentication;

public sealed record AuthenticatedUserV1(
    string UserId,
    string? Email,
    string? UserName,
    IReadOnlyList<string> Roles);
