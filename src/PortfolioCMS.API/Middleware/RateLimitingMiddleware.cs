using System.Collections.Concurrent;

namespace PortfolioCMS.API.Middleware;

/// <summary>
/// Middleware to implement rate limiting for API endpoints
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
    private static readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);
    private const int _maxRequestsPerWindow = 100;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        // Cleanup old entries periodically
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(5));
                CleanupOldEntries();
            }
        });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var clientInfo = _clients.GetOrAdd(clientId, _ => new ClientRequestInfo
        {
            FirstRequestTime = now,
            RequestCount = 0
        });

        bool rateLimitExceeded = false;
        
        lock (clientInfo)
        {
            // Reset counter if time window has passed
            if (now - clientInfo.FirstRequestTime > _timeWindow)
            {
                clientInfo.FirstRequestTime = now;
                clientInfo.RequestCount = 0;
            }

            clientInfo.RequestCount++;

            // Check if rate limit exceeded
            if (clientInfo.RequestCount > _maxRequestsPerWindow)
            {
                rateLimitExceeded = true;
            }
        }

        if (rateLimitExceeded)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client: {ClientId}, IP: {IP}",
                clientId,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Append("Retry-After", "60");
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "RATE_LIMIT_EXCEEDED",
                    message = "Too many requests. Please try again later.",
                    retryAfter = 60,
                    timestamp = DateTime.UtcNow,
                    traceId = context.TraceIdentifier
                }
            });
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get client IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // If behind a proxy, try to get the real IP
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ipAddress = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim() ?? ipAddress;
        }

        // Combine IP with User-Agent for better identification
        var userAgent = context.Request.Headers.UserAgent.ToString();
        return $"{ipAddress}:{userAgent.GetHashCode()}";
    }

    private static void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _clients
            .Where(kvp => now - kvp.Value.FirstRequestTime > TimeSpan.FromMinutes(10))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _clients.TryRemove(key, out _);
        }
    }

    private class ClientRequestInfo
    {
        public DateTime FirstRequestTime { get; set; }
        public int RequestCount { get; set; }
    }
}

/// <summary>
/// Extension methods for registering RateLimitingMiddleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
