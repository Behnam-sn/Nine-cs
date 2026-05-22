namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class HashedSecretCannotBeEmptyException : Exception
{
    public HashedSecretCannotBeEmptyException() : base() { }
}
