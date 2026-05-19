namespace Nine.Identities.Domain.Accounts.Exceptions;

public sealed class HashedSecretCannotBeEmptyException : Exception
{
    public HashedSecretCannotBeEmptyException() : base() { }
}