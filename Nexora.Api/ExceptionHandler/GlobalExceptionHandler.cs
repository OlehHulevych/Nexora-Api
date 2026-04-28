using System.Security.Principal;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nexora.Domain.Exceptions;

namespace Nexora.Api.ExceptionHandler;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred:{Message}", exception.Message);
        var (statusCode, title, errors) = MapException(exception);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };
        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;

    }

    private static (int statusCode, string title, object? errors) MapException(Exception exception)
    {
        return exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", null),
            ValidationException ex => (StatusCodes.Status400BadRequest, "Validation Error", ex.Errors),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", null),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", null),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", null),
            ArgumentException => (StatusCodes.Status400BadRequest, "BadRequest", null),
            PasswordIsNotMatched => (StatusCodes.Status400BadRequest, "Password is not matched", null),
            UserAlreadyExistsException => (StatusCodes.Status400BadRequest, "User with this email already exists", null),
            UserIsNotFoundException => (StatusCodes.Status400BadRequest, "User is not exist", null),
        _ => (StatusCodes.Status500InternalServerError, "Server internal error", null)
        };
    }
}