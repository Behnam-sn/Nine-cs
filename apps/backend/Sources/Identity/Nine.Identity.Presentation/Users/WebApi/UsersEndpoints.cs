using System.Security.Claims;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Nine.Identity.Application.Users.Commands.CreateUserWithPassword;
using Nine.Identity.Presentation.Users.WebApi.Requests;
using Nine.Identity.Presentation.Users.WebApi.Responses;
using Nine.Shared.Application.Abstractions.Messaging;
using Nine.Shared.Presentation.Common.Security;

namespace Nine.Identity.Presentation.Users.WebApi;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersWebApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/UsersWebApi", CreateWithPassword)
            .AllowAnonymous();

        endpoints.MapGet("/api/v1/me", GetMe)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> CreateWithPassword(
        CreateUserWithPasswordRequestV1 request,
        ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserWithPasswordCommandV1(
            EmailAddress: request.EmailAddress,
            Password: request.Password,
            PhoneNumber: request.PhoneNumber
        );

        var userId = await commandBus.Send(command, cancellationToken);

        return Results.Json(
            new CreateUserWithPasswordResponseV1(userId.ToString()),
            statusCode: StatusCodes.Status201Created);
    }

    private static IResult GetMe(ClaimsPrincipal user)
    {
        var userId = user.FindUserId() ?? string.Empty;
        var email = user.FindEmail();

        return Results.Ok(new MeResponseV1(userId, email));
    }
}
