using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nine.Identities.Domain.Contracts.Accounts.Exceptions;

namespace Nine.Identities.Presentation.Accounts.WebApi.ExceptionHandlers;

public sealed class AccountExceptionHandler : IExceptionHandler
{
    private readonly ILogger<AccountExceptionHandler> _logger;

    public AccountExceptionHandler(ILogger<AccountExceptionHandler> logger)
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
            AccountEmailAddressAlreadyInUseException
                => (StatusCodes.Status409Conflict, "Email address already in use"),

            AccountPhoneNumberAlreadyInUseException
                => (StatusCodes.Status409Conflict, "Phone number already in use"),

            CredentialAlreadyExistsException
                => (StatusCodes.Status409Conflict, "Credential already exists"),

            CredentialNotFoundException
                => (StatusCodes.Status404NotFound, "Credential not found"),

            AccountPhoneNumberNotSetException
                => (StatusCodes.Status400BadRequest, "Phone number is not set"),

            CannotRemoveLastCredentialException
                => (StatusCodes.Status400BadRequest, "Cannot remove last credential"),

            ArgumentException
                => (StatusCodes.Status400BadRequest, "Invalid request"),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}