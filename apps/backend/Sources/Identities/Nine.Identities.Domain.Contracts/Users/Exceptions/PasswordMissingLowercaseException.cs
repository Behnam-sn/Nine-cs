namespace Nine.Identities.Domain.Contracts.Users.Exceptions;

public sealed class PasswordMissingLowercaseException : Exception
{
    public PasswordMissingLowercaseException()
        : base("Password must contain at least one lowercase letter.")
    {
    }
}
