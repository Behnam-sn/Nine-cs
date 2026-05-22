namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class CredentialIdCannotBeEmptyException : Exception
{
    public CredentialIdCannotBeEmptyException() : base() { }
}
