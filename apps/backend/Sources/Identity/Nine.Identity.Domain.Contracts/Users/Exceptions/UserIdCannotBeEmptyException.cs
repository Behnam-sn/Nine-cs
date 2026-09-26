namespace Nine.Identity.Domain.Contracts.Users.Exceptions;

public sealed class UserIdCannotBeEmptyException : Exception
{
    public UserIdCannotBeEmptyException() : base() { }
}
