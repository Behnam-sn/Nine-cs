namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class PasswordMissingLowercaseException : Exception
{
    public PasswordMissingLowercaseException()
        : base("Password must contain at least one lowercase letter.")
    {
    }
}