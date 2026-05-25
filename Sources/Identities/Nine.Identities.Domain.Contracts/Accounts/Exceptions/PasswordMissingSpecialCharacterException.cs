namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class PasswordMissingSpecialCharacterException : Exception
{
    public PasswordMissingSpecialCharacterException()
        : base("Password must contain at least one special character.")
    {
    }
}