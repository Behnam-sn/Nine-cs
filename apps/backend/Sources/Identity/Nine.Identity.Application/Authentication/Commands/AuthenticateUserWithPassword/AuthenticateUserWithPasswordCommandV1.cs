using Nine.Shared.Application.Abstractions.Messaging;

namespace Nine.Identity.Application.Authentication.Commands.AuthenticateUserWithPassword;

public sealed record AuthenticateUserWithPasswordCommandV1(string Username, string Password)
    : ICommand<AuthenticateUserResult>;
