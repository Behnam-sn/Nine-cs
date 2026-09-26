namespace Nine.Identity.Domain.Contracts.Users.Exceptions;

public sealed class PasswordTooShortException : Exception
{
    public int MinimumLength { get; }

    public PasswordTooShortException(int minimumLength)
        : base($"Password must be at least {minimumLength} characters.")
    {
        MinimumLength = minimumLength;
    }
}
