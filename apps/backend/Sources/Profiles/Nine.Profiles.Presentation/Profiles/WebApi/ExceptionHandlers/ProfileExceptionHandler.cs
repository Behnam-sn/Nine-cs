using System.Diagnostics;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;

namespace Nine.Profiles.Presentation.Profiles.WebApi.ExceptionHandlers;

public sealed class ProfileExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ProfileExceptionHandler> _logger;

    public ProfileExceptionHandler(ILogger<ProfileExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        _logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path
        );

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
            }
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception)
    {
        return exception switch
        {
            ProfileHandleAlreadyInUseException
                => (StatusCodes.Status409Conflict, "Profile handle already in use"),

            ProfileHandleInvalidCharactersException or ProfileHandleTooLongException
                or ProfileHandleTooShortException
                => (StatusCodes.Status400BadRequest, "Invalid profile handle"),

            ProfileIdCannotBeEmptyException or ProfileIdInvalidFormatException
                => (StatusCodes.Status400BadRequest, "Invalid profile ID"),

            ProfileNameCannotBeEmptyException or ProfileNameTooLongException
                => (StatusCodes.Status400BadRequest, "Invalid profile name"),

            ProfileBioTooLongException
                => (StatusCodes.Status400BadRequest, "Invalid profile bio"),

            ProfileAvatarObjectKeyCannotBeEmptyException or ProfileAvatarInvalidMediaTypeException
                => (StatusCodes.Status400BadRequest, "Invalid profile avatar"),

            ArgumentException
                => (StatusCodes.Status400BadRequest, "Invalid request"),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}
