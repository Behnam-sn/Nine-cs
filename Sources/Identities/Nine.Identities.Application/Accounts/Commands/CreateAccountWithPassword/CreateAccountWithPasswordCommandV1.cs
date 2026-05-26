using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Application.Accounts.Commands.CreateAccountWithPassword;

public sealed record CreateAccountWithPasswordCommandV1(
    string EmailAddress,
    string Password,
    string? PhoneNumber = null
) : ICommand<AccountId>;
