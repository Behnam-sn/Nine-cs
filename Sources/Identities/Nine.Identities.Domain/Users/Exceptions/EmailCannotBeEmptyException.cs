namespace Nine.Identities.Domain.Users.Exceptions;

public sealed class EmailCannotBeEmptyException : Exception
{
    public EmailCannotBeEmptyException() : base() { }
}