namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class CredentialAlreadyExistsException : Exception
{
    public CredentialAlreadyExistsException() : base() { }
}
