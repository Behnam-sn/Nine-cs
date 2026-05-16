namespace Nine.Identities.Domain.Users.Exceptions;

public sealed class NameCannotBeEmptyException : Exception
{
    public NameCannotBeEmptyException() : base() { }
}