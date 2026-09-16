using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Application.Authentication.Commands.AuthenticateUserWithPassword;

public sealed record AuthenticateUserWithPasswordCommandV1(string Username, string Password)
    : ICommand<AuthenticateUserResult>;
