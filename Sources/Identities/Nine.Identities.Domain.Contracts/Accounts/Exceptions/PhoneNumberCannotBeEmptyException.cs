namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class PhoneNumberCannotBeEmptyException : Exception
{
    public PhoneNumberCannotBeEmptyException() : base() { }
}
