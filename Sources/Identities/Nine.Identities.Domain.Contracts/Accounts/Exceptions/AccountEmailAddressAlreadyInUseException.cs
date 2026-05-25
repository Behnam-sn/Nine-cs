using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class AccountEmailAddressAlreadyInUseException : Exception
{
    public AccountEmailAddressAlreadyInUseException(EmailAddress emailAddress)
        : base($"The {emailAddress.Value} Email Address Already In Use")
    {
    }
}