namespace Nine.Identity.Domain.Contracts.Users.Exceptions;

public sealed class EmailAddressCannotBeEmptyException : Exception
{
    public EmailAddressCannotBeEmptyException() : base() { }
}
