namespace Nine.Identities.Domain.Accounts.Exceptions;

public sealed class EmailAddressCannotBeEmptyException : Exception
{
    public EmailAddressCannotBeEmptyException() : base() { }
}