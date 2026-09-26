using Microsoft.AspNetCore.Identity;

using Nine.Identity.Domain.Users.Entities;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identity.Application.Authentication.Commands.AuthenticateUserWithPassword;

public sealed class AuthenticateUserWithPasswordCommandV1Handler
    : ICommandHandler<AuthenticateUserWithPasswordCommandV1, AuthenticateUserResult>
{
    private readonly UserManager<User> _userManager;

    public AuthenticateUserWithPasswordCommandV1Handler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthenticateUserResult> Handle(
        AuthenticateUserWithPasswordCommandV1 request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthenticateUserResult.InvalidCredentials();
        }

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            return AuthenticateUserResult.InvalidCredentials();
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return AuthenticateUserResult.InvalidCredentials();
        }

        if (!await UserAuthentication.CanSignInAsync(_userManager, user))
        {
            return AuthenticateUserResult.CannotSignIn();
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var authenticatedUser = await UserAuthentication.ToAuthenticatedUserAsync(_userManager, user);
        return AuthenticateUserResult.Succeeded(authenticatedUser);
    }
}
