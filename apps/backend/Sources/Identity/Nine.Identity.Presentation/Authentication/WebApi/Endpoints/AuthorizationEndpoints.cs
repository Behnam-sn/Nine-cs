using System.Security.Claims;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;

using Nine.Identity.Application.Authentication;
using Nine.Identity.Application.Authentication.Commands.AuthenticateUserWithPassword;
using Nine.Identity.Application.Authentication.Queries.GetUserForSignIn;
using Nine.Shared.Application.Abstractions.Messaging;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Nine.Identity.Presentation.Authentication.WebApi.Endpoints;

public static class AuthorizationEndpoints
{
    public static IEndpointRouteBuilder MapAuthorizationWebApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", [HttpMethods.Get, HttpMethods.Post], Authorize);
        endpoints.MapPost("/connect/token", Exchange);
        endpoints.MapMethods("/connect/logout", [HttpMethods.Get, HttpMethods.Post], Logout);

        return endpoints;
    }

    private static async Task<IResult> Authorize(
        HttpContext httpContext,
        IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var request = httpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result is not { Succeeded: true, Principal: { } principal })
        {
            return Results.Forbid(
                new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                }),
                [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? principal.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return InvalidGrant("The user is not logged in.");
        }

        var authentication = await queryBus.Send(new GetUserForSignInQueryV1(userId), cancellationToken);
        if (authentication.Status != AuthenticateUserStatus.Succeeded || authentication.User is null)
        {
            return InvalidGrant("The user is no longer allowed to sign in.");
        }

        var identity = CreateIdentity(authentication.User, request.GetScopes());
        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> Exchange(
        HttpContext httpContext,
        ICommandBus commandBus,
        IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var request = httpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            var authentication = await commandBus.Send(
                new AuthenticateUserWithPasswordCommandV1(request.Username!, request.Password!),
                cancellationToken);

            return authentication.Status switch
            {
                AuthenticateUserStatus.Succeeded when authentication.User is not null =>
                    Results.SignIn(
                        new ClaimsPrincipal(CreateIdentity(authentication.User, request.GetScopes())),
                        authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme),
                AuthenticateUserStatus.CannotSignIn =>
                    InvalidGrant("The user is no longer allowed to sign in."),
                _ => InvalidGrant("The username or password is invalid.")
            };
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var userId = result.Principal?.GetClaim(Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                return InvalidGrant("The token is no longer valid.");
            }

            var authentication = await queryBus.Send(new GetUserForSignInQueryV1(userId), cancellationToken);
            return authentication.Status switch
            {
                AuthenticateUserStatus.Succeeded when authentication.User is not null =>
                    Results.SignIn(
                        new ClaimsPrincipal(CreateIdentity(authentication.User, request.GetScopes())),
                        authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme),
                AuthenticateUserStatus.CannotSignIn =>
                    InvalidGrant("The user is no longer allowed to sign in."),
                _ => InvalidGrant("The token is no longer valid.")
            };
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    private static async Task<IResult> Logout(SignInManager<Domain.Users.Entities.User> signInManager)
    {
        await signInManager.SignOutAsync();

        return Results.SignOut(
            new AuthenticationProperties
            {
                RedirectUri = "/"
            },
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static IResult InvalidGrant(string description)
    {
        return Results.Forbid(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static ClaimsIdentity CreateIdentity(AuthenticatedUserV1 user, IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, user.UserId)
            .SetClaim(Claims.Email, user.Email)
            .SetClaim(Claims.EmailVerified, user.EmailVerified)
            .SetClaim(Claims.Name, user.UserName)
            .SetClaim(Claims.PreferredUsername, user.UserName)
            .SetClaims(Claims.Role, [.. user.Roles]);

        identity.SetScopes(scopes);
        identity.SetDestinations(GetDestinations);

        return identity;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;
                if (claim.Subject!.HasScope(Scopes.Profile))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Email or Claims.EmailVerified:
                yield return Destinations.AccessToken;
                if (claim.Subject!.HasScope(Scopes.Email))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (claim.Subject!.HasScope(Scopes.Roles))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
