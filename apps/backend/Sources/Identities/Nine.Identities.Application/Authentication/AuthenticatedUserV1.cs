namespace Nine.Identities.Application.Authentication;

public sealed record AuthenticatedUserV1(
    string UserId,
    string? Email,
    bool EmailVerified,
    string? UserName,
    IReadOnlyList<string> Roles);
