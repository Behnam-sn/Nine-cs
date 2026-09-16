namespace Nine.Identities.Domain.Contracts.Users.Exceptions;

public sealed class PasswordEmptyException : Exception
{
    public PasswordEmptyException()
        : base("Password cannot be empty.")
    {
    }
}
