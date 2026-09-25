using Nine.Identities.Domain.Contracts.Users.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Users.Exceptions;

public sealed class UserPhoneNumberAlreadyInUseException : Exception
{
    public UserPhoneNumberAlreadyInUseException(PhoneNumber phoneNumber)
        : base($"The {phoneNumber.Value} Phone Number Already In Use")
    {
    }
}
