namespace Nine.Identities.Presentation.Accounts.WebApi.Requests;

public sealed record CreateAccountWithPasswordRequestV1(
    string EmailAddress,
    string? PhoneNumber,
    string Password
);
