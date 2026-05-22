namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class CannotRemoveLastCredentialException : Exception
{
    public CannotRemoveLastCredentialException() : base() { }
}
