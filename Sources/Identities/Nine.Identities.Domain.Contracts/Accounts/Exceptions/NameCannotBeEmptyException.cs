namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class NameCannotBeEmptyException : Exception
{
    public NameCannotBeEmptyException() : base() { }
}
