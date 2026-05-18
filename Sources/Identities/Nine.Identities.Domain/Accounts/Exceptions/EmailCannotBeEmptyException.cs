namespace Nine.Identities.Domain.Accounts.Exceptions;

public sealed class EmailCannotBeEmptyException : Exception
{
    public EmailCannotBeEmptyException() : base() { }
}