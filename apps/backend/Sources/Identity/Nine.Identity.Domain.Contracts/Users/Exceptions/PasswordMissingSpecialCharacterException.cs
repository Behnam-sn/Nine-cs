namespace Nine.Identity.Domain.Contracts.Users.Exceptions;

public sealed class PasswordMissingSpecialCharacterException : Exception
{
    public PasswordMissingSpecialCharacterException()
        : base("Password must contain at least one special character.")
    {
    }
}
