using Nine.Identity.Domain.Contracts.Users.ValueObjects;
using Nine.Shared.Application.Abstractions.Messaging;

namespace Nine.Identity.Application.Users.Commands.CreateUserWithPassword;

public sealed record CreateUserWithPasswordCommandV1(
    string EmailAddress,
    string Password,
    string? PhoneNumber = null
) : ICommand<UserId>;
