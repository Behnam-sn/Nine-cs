using Nine.Identity.Domain.Contracts.Users.ValueObjects;

namespace Nine.Identity.Domain.Contracts.Users.Exceptions;

public sealed class UserPhoneNumberAlreadyInUseException : Exception
{
    public UserPhoneNumberAlreadyInUseException(PhoneNumber phoneNumber)
        : base($"The {phoneNumber.Value} Phone Number Already In Use")
    {
    }
}
