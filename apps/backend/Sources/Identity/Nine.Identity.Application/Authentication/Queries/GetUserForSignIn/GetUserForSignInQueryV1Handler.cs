using Microsoft.AspNetCore.Identity;

using Nine.Identity.Domain.Users.Entities;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identity.Application.Authentication.Queries.GetUserForSignIn;

public sealed class GetUserForSignInQueryV1Handler : IQueryHandler<GetUserForSignInQueryV1, AuthenticateUserResult>
{
    private readonly UserManager<User> _userManager;

    public GetUserForSignInQueryV1Handler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthenticateUserResult> Handle(GetUserForSignInQueryV1 request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return AuthenticateUserResult.NotFound();
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return AuthenticateUserResult.NotFound();
        }

        if (!await UserAuthentication.CanSignInAsync(_userManager, user))
        {
            return AuthenticateUserResult.CannotSignIn();
        }

        var authenticatedUser = await UserAuthentication.ToAuthenticatedUserAsync(_userManager, user);
        return AuthenticateUserResult.Succeeded(authenticatedUser);
    }
}
