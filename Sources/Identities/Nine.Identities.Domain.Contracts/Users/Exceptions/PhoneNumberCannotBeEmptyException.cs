namespace Nine.Identities.Domain.Contracts.Users.Exceptions;

public sealed class PhoneNumberCannotBeEmptyException : Exception
{
    public PhoneNumberCannotBeEmptyException() : base() { }
}
