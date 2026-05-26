namespace Nine.Identities.Presentation.Accounts.WebApi.Requests;

public sealed record CreateAccountWithPasswordRequest(
    string EmailAddress,
    string? PhoneNumber,
    string Password
);
