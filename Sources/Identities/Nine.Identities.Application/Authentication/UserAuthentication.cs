using Microsoft.AspNetCore.Identity;

using Nine.Identities.Domain.Users.Entities;

namespace Nine.Identities.Application.Authentication;

internal static class UserAuthentication
{
    public static async Task<bool> CanSignInAsync(UserManager<User> userManager, User user)
    {
        if (await userManager.IsLockedOutAsync(user))
        {
            return false;
        }

        if (userManager.Options.SignIn.RequireConfirmedEmail && !await userManager.IsEmailConfirmedAsync(user))
        {
            return false;
        }

        if (userManager.Options.SignIn.RequireConfirmedPhoneNumber && !await userManager.IsPhoneNumberConfirmedAsync(user))
        {
            return false;
        }

        return true;
    }

    public static async Task<AuthenticatedUserV1> ToAuthenticatedUserAsync(UserManager<User> userManager, User user)
    {
        return new AuthenticatedUserV1(
            UserId: await userManager.GetUserIdAsync(user),
            Email: await userManager.GetEmailAsync(user),
            EmailVerified: await userManager.IsEmailConfirmedAsync(user),
            UserName: await userManager.GetUserNameAsync(user),
            Roles: [.. await userManager.GetRolesAsync(user)]);
    }
}
