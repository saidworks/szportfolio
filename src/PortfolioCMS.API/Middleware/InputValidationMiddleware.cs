using System.Text;
using System.Text.RegularExpressions;

namespace PortfolioCMS.API.Middleware;

/// <summary>
/// Middleware to validate and sanitize incoming requests for security threats
/// </summary>
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;

    // Patterns for detecting common attack vectors
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|DECLARE)\b)|(-{2})|(/\*)|(\*/)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new(
        @"<script|javascript:|onerror=|onload=|<iframe|eval\(|expression\(",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PathTraversalPattern = new(
        @"\.\./|\.\.\\|%2e%2e|%252e%252e",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public InputValidationMiddleware(
        RequestDelegate next,
        ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Validate query strings
        if (context.Request.Query.Any())
        {
            foreach (var query in context.Request.Query)
            {
                if (ContainsMaliciousContent(query.Value.ToString()))
                {
                    _logger.LogWarning(
                        "Malicious content detected in query parameter: {Key} from IP: {IP}",
                        query.Key,
                        context.Connection.RemoteIpAddress);

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = new
                        {
                            code = "INVALID_INPUT",
                            message = "Invalid input detected in request",
                            timestamp = DateTime.UtcNow,
                            traceId = context.TraceIdentifier
                        }
                    });
                    return;
                }
            }
        }

        // Validate headers for suspicious content
        if (ContainsSuspiciousHeaders(context.Request.Headers))
        {
            _logger.LogWarning(
                "Suspicious headers detected from IP: {IP}",
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "INVALID_HEADERS",
                    message = "Invalid headers detected in request",
                    timestamp = DateTime.UtcNow,
                    traceId = context.TraceIdentifier
                }
            });
            return;
        }

        // Validate request path for path traversal attempts
        if (PathTraversalPattern.IsMatch(context.Request.Path.Value ?? string.Empty))
        {
            _logger.LogWarning(
                "Path traversal attempt detected: {Path} from IP: {IP}",
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "INVALID_PATH",
                    message = "Invalid path detected in request",
                    timestamp = DateTime.UtcNow,
                    traceId = context.TraceIdentifier
                }
            });
            return;
        }

        await _next(context);
    }

    private static bool ContainsMaliciousContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Check for SQL injection patterns
        if (SqlInjectionPattern.IsMatch(input))
            return true;

        // Check for XSS patterns
        if (XssPattern.IsMatch(input))
            return true;

        // Check for path traversal
        if (PathTraversalPattern.IsMatch(input))
            return true;

        return false;
    }

    private static bool ContainsSuspiciousHeaders(IHeaderDictionary headers)
    {
        // Check for suspicious User-Agent patterns
        if (headers.TryGetValue("User-Agent", out var userAgent))
        {
            var ua = userAgent.ToString().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ua) ||
                ua.Contains("sqlmap") ||
                ua.Contains("nikto") ||
                ua.Contains("nmap") ||
                ua.Contains("masscan"))
            {
                return true;
            }
        }

        // Check for suspicious custom headers
        foreach (var header in headers)
        {
            if (ContainsMaliciousContent(header.Value.ToString()))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Extension methods for registering InputValidationMiddleware
/// </summary>
public static class InputValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<InputValidationMiddleware>();
    }
}
