namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class PasswordEmptyException : Exception
{
    public PasswordEmptyException()
        : base("Password cannot be empty.")
    {
    }
}