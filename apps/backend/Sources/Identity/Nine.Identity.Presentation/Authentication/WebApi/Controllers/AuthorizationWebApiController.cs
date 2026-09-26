using System.Security.Claims;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Nine.Identity.Application.Authentication;
using Nine.Identity.Application.Authentication.Commands.AuthenticateUserWithPassword;
using Nine.Identity.Application.Authentication.Queries.GetUserForSignIn;
using Nine.SharedKernel.Abstractions.Messaging;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Nine.Identity.Presentation.Authentication.WebApi.Controllers;

[ApiController]
public sealed class AuthorizationWebApiController : ControllerBase
{
    private readonly ICommandBus _commandBus;
    private readonly IQueryBus _queryBus;
    private readonly SignInManager<Domain.Users.Entities.User> _signInManager;

    public AuthorizationWebApiController(
        ICommandBus commandBus,
        IQueryBus queryBus,
        SignInManager<Domain.Users.Entities.User> signInManager)
    {
        _commandBus = commandBus;
        _queryBus = queryBus;
        _signInManager = signInManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result is not { Succeeded: true, Principal: { } principal })
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                }));
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? principal.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return InvalidGrant("The user is not logged in.");
        }

        var authentication = await _queryBus.Send(new GetUserForSignInQueryV1(userId), cancellationToken);
        if (authentication.Status != AuthenticateUserStatus.Succeeded || authentication.User is null)
        {
            return InvalidGrant("The user is no longer allowed to sign in.");
        }

        var identity = CreateIdentity(authentication.User, request.GetScopes());
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            var authentication = await _commandBus.Send(
                new AuthenticateUserWithPasswordCommandV1(request.Username!, request.Password!),
                cancellationToken);

            return authentication.Status switch
            {
                AuthenticateUserStatus.Succeeded when authentication.User is not null =>
                    SignIn(
                        new ClaimsPrincipal(CreateIdentity(authentication.User, request.GetScopes())),
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme),
                AuthenticateUserStatus.CannotSignIn =>
                    InvalidGrant("The user is no longer allowed to sign in."),
                _ => InvalidGrant("The username or password is invalid.")
            };
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var userId = result.Principal?.GetClaim(Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                return InvalidGrant("The token is no longer valid.");
            }

            var authentication = await _queryBus.Send(new GetUserForSignInQueryV1(userId), cancellationToken);
            return authentication.Status switch
            {
                AuthenticateUserStatus.Succeeded when authentication.User is not null =>
                    SignIn(
                        new ClaimsPrincipal(CreateIdentity(authentication.User, request.GetScopes())),
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme),
                AuthenticateUserStatus.CannotSignIn =>
                    InvalidGrant("The user is no longer allowed to sign in."),
                _ => InvalidGrant("The token is no longer valid.")
            };
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    private IActionResult InvalidGrant(string description)
    {
        return Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }));
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
