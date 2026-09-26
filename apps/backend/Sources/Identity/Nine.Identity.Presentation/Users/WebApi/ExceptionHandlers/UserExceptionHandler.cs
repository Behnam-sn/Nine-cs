using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Nine.Identity.Domain.Contracts.Users.Exceptions;

namespace Nine.Identity.Presentation.Users.WebApi.ExceptionHandlers;

public sealed class UserExceptionHandler : IExceptionHandler
{
    private readonly ILogger<UserExceptionHandler> _logger;

    public UserExceptionHandler(ILogger<UserExceptionHandler> logger)
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
            UserEmailAddressAlreadyInUseException
                => (StatusCodes.Status409Conflict, "Email address already in use"),

            UserPhoneNumberAlreadyInUseException
                => (StatusCodes.Status409Conflict, "Phone number already in use"),

            EmailAddressCannotBeEmptyException or EmailAddressInvalidFormatException
                => (StatusCodes.Status400BadRequest, "Invalid email address"),

            PhoneNumberCannotBeEmptyException or PhoneNumberInvalidFormatException
                => (StatusCodes.Status400BadRequest, "Invalid phone number"),

            PasswordEmptyException or PasswordTooShortException or PasswordHavingWhiteSpaceException
                or PasswordMissingUppercaseException or PasswordMissingLowercaseException
                or PasswordMissingDigitException or PasswordMissingSpecialCharacterException
                => (StatusCodes.Status400BadRequest, "Invalid password"),

            ArgumentException
                => (StatusCodes.Status400BadRequest, "Invalid request"),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}
