namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class AccountIdCannotBeEmptyException : Exception
{
    public AccountIdCannotBeEmptyException() : base() { }
}
