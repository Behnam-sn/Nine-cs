namespace Nine.Identities.Domain.Contracts.Accounts.Exceptions;

public sealed class PasswordTooShortException : Exception
{
    public int MinimumLength { get; }

    public PasswordTooShortException(int minimumLength)
        : base($"Password must be at least {minimumLength} characters.")
    {
        MinimumLength = minimumLength;
    }
}