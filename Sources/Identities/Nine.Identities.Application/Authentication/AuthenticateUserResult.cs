namespace Nine.Identities.Application.Authentication;

public enum AuthenticateUserStatus
{
    Succeeded,
    InvalidCredentials,
    CannotSignIn,
    NotFound
}

public sealed record AuthenticateUserResult(AuthenticateUserStatus Status, AuthenticatedUserV1? User)
{
    public static AuthenticateUserResult Succeeded(AuthenticatedUserV1 user)
    {
        return new(AuthenticateUserStatus.Succeeded, user);
    }

    public static AuthenticateUserResult InvalidCredentials()
    {
        return new(AuthenticateUserStatus.InvalidCredentials, null);
    }

    public static AuthenticateUserResult CannotSignIn()
    {
        return new(AuthenticateUserStatus.CannotSignIn, null);
    }

    public static AuthenticateUserResult NotFound()
    {
        return new(AuthenticateUserStatus.NotFound, null);
    }
}
