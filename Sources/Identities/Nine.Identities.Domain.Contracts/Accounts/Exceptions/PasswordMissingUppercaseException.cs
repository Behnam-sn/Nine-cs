namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class PasswordMissingUppercaseException : Exception
{
    public PasswordMissingUppercaseException()
        : base("Password must contain at least one uppercase letter.")
    {
    }
}

public sealed class PasswordHavingWhiteSpaceException : Exception
{
    public PasswordHavingWhiteSpaceException()
        : base("Password must not contain any white space.")
    {
    }
}