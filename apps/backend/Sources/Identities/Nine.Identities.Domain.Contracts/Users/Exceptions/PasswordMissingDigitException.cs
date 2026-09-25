namespace Nine.Identities.Domain.Contracts.Users.Exceptions;

public sealed class PasswordMissingDigitException : Exception
{
    public PasswordMissingDigitException()
        : base("Password must contain at least one digit.")
    {
    }
}
