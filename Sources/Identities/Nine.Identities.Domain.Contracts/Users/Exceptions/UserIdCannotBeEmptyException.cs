namespace Nine.Identities.Domain.Contracts.Users.Exceptions;

public sealed class UserIdCannotBeEmptyException : Exception
{
    public UserIdCannotBeEmptyException() : base() { }
}
