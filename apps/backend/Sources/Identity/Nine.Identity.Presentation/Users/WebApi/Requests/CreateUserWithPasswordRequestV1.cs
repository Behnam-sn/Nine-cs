namespace Nine.Identity.Presentation.Users.WebApi.Requests;

public sealed record CreateUserWithPasswordRequestV1(
    string EmailAddress,
    string? PhoneNumber,
    string Password
);
