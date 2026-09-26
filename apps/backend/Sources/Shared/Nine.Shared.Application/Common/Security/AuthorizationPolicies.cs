namespace Nine.Shared.Application.Common.Security;

public static class AuthorizationPolicies
{
    public const string Authenticated = "Authenticated";
    public const string MustHaveVerifiedEmail = "MustHaveVerifiedEmail";
    public const string MustBeMember = "MustBeMember";
    public const string MustBeModerator = "MustBeModerator";
}
