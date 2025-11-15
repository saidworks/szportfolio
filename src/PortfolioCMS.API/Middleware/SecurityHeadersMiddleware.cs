using Microsoft.Extensions.Primitives;

namespace PortfolioCMS.API.Middleware;

/// <summary>
/// Middleware to add comprehensive security headers to HTTP responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        ILogger<SecurityHeadersMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing the request
        AddSecurityHeaders(context);

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Content Security Policy (CSP)
        // Restricts sources for scripts, styles, images, etc.
        var cspPolicy = "default-src 'self'; " +
                       "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                       "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                       "img-src 'self' data: https: blob:; " +
                       "font-src 'self' https://fonts.gstatic.com data:; " +
                       "connect-src 'self' https://dc.services.visualstudio.com; " +
                       "frame-ancestors 'none'; " +
                       "base-uri 'self'; " +
                       "form-action 'self'";

        headers.Append("Content-Security-Policy", cspPolicy);

        // X-Content-Type-Options
        // Prevents MIME type sniffing
        headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options
        // Prevents clickjacking attacks
        headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection
        // Enables XSS filter in older browsers
        headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy
        // Controls how much referrer information is included with requests
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy (formerly Feature-Policy)
        // Controls which browser features can be used
        headers.Append("Permissions-Policy", 
            "geolocation=(), " +
            "microphone=(), " +
            "camera=(), " +
            "payment=(), " +
            "usb=(), " +
            "magnetometer=(), " +
            "gyroscope=(), " +
            "accelerometer=()");

        // HTTP Strict Transport Security (HSTS)
        // Forces HTTPS connections (only in production)
        if (!_environment.IsDevelopment())
        {
            headers.Append("Strict-Transport-Security", 
                "max-age=31536000; includeSubDomains; preload");
        }

        // Remove server header to avoid information disclosure
        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");
        headers.Remove("X-AspNetMvc-Version");

        _logger.LogDebug("Security headers added to response for {Path}", context.Request.Path);
    }
}

/// <summary>
/// Extension methods for registering SecurityHeadersMiddleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
