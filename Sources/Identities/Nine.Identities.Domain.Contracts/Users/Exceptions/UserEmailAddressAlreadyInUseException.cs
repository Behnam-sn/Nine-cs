using Nine.Identities.Domain.Contracts.Users.ValueObjects;

namespace Nine.Identities.Domain.Contracts.Users.Exceptions;

public sealed class UserEmailAddressAlreadyInUseException : Exception
{
    public UserEmailAddressAlreadyInUseException(EmailAddress emailAddress)
        : base($"The {emailAddress.Value} Email Address Already In Use")
    {
    }
}
