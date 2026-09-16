using Nine.Identities.Domain.Contracts.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Application.Users.Commands.CreateUserWithPassword;

public sealed record CreateUserWithPasswordCommandV1(
    string EmailAddress,
    string Password,
    string? PhoneNumber = null
) : ICommand<UserId>;
