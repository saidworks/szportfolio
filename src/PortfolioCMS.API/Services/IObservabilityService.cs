namespace PortfolioCMS.API.Services;

public interface IObservabilityService
{
    /// <summary>
    /// Track a custom event with optional properties
    /// </summary>
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Track an exception with optional properties
    /// </summary>
    void TrackException(Exception exception, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Track a custom metric with optional properties
    /// </summary>
    void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Track a dependency call (external service, database, etc.)
    /// </summary>
    void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success);

    /// <summary>
    /// Track a page view
    /// </summary>
    void TrackPageView(string pageName, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Track a request with timing information
    /// </summary>
    void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success);

    /// <summary>
    /// Start a timed operation for tracking
    /// </summary>
    IDisposable StartOperation(string operationName, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Set user context for telemetry
    /// </summary>
    void SetUser(string userId, string? accountId = null);

    /// <summary>
    /// Add custom properties to all telemetry
    /// </summary>
    void AddGlobalProperty(string key, string value);

    /// <summary>
    /// Flush all telemetry data immediately
    /// </summary>
    Task FlushAsync();
}