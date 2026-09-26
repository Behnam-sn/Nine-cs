using Nine.Identity.Domain.Contracts.Users.ValueObjects;

namespace Nine.Identity.Domain.Contracts.Users.Exceptions;

public sealed class UserEmailAddressAlreadyInUseException : Exception
{
    public UserEmailAddressAlreadyInUseException(EmailAddress emailAddress)
        : base($"The {emailAddress.Value} Email Address Already In Use")
    {
    }
}
