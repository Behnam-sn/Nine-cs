using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class AccountPhoneNumberAlreadyInUseException : Exception
{
    public AccountPhoneNumberAlreadyInUseException(PhoneNumber phoneNumber)
        : base($"The {phoneNumber.Value} Phone Number Already In Use")
    {
    }
}