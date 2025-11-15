using FluentValidation;
using System.Net;
using System.Text.Json;
using PortfolioCMS.API.Services;

namespace PortfolioCMS.API.Middleware;

/// <summary>
/// Global exception handling middleware with observability integration
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IObservabilityService observability)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred for {Path}", context.Request.Path);
            
            observability.TrackException(ex, new Dictionary<string, string>
            {
                ["ErrorType"] = "Validation",
                ["RequestPath"] = context.Request.Path,
                ["Method"] = context.Request.Method
            });

            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt for {Path}", context.Request.Path);
            
            observability.TrackException(ex, new Dictionary<string, string>
            {
                ["ErrorType"] = "Authorization",
                ["RequestPath"] = context.Request.Path,
                ["Method"] = context.Request.Method
            });

            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found for {Path}", context.Request.Path);
            
            observability.TrackException(ex, new Dictionary<string, string>
            {
                ["ErrorType"] = "NotFound",
                ["RequestPath"] = context.Request.Path,
                ["Method"] = context.Request.Method
            });

            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation for {Path}", context.Request.Path);
            
            observability.TrackException(ex, new Dictionary<string, string>
            {
                ["ErrorType"] = "InvalidOperation",
                ["RequestPath"] = context.Request.Path,
                ["Method"] = context.Request.Method
            });

            await HandleInvalidOperationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred for {Path}", context.Request.Path);
            
            observability.TrackException(ex, new Dictionary<string, string>
            {
                ["ErrorType"] = "Unhandled",
                ["RequestPath"] = context.Request.Path,
                ["Method"] = context.Request.Method
            });

            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            error = new
            {
                code = "VALIDATION_ERROR",
                message = "One or more validation errors occurred",
                details = errors,
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                instance = context.Request.Path.Value
            }
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        var response = new
        {
            error = new
            {
                code = "FORBIDDEN",
                message = "You do not have permission to access this resource",
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                instance = context.Request.Path.Value
            }
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleNotFoundExceptionAsync(HttpContext context, KeyNotFoundException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var response = new
        {
            error = new
            {
                code = "NOT_FOUND",
                message = exception.Message,
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                instance = context.Request.Path.Value
            }
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleInvalidOperationExceptionAsync(HttpContext context, InvalidOperationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            error = new
            {
                code = "INVALID_OPERATION",
                message = exception.Message,
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                instance = context.Request.Path.Value
            }
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            error = new
            {
                code = "INTERNAL_SERVER_ERROR",
                message = "An unexpected error occurred. Please try again later.",
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                instance = context.Request.Path.Value
            }
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension methods for registering GlobalExceptionMiddleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
