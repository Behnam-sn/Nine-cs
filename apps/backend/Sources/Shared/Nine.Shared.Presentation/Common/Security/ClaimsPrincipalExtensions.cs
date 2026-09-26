using System.Security.Claims;

using Nine.Shared.Application.Common.Security;

namespace Nine.Shared.Presentation.Common.Security;

public static class ClaimsPrincipalExtensions
{
    public static string? FindUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimNames.UserId)
               ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string? FindEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimNames.Email)
               ?? user.FindFirstValue(ClaimTypes.Email);
    }

    public static bool HasVerifiedEmail(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimNames.EmailVerified);
        return bool.TryParse(value, out var verified) && verified;
    }

    public static string? FindProfileId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimNames.ProfileId);
    }
}
