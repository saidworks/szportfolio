namespace PortfolioCMS.API.Services;

/// <summary>
/// No-operation implementation of IObservabilityService for when Application Insights is not configured
/// </summary>
public class NoOpObservabilityService : IObservabilityService
{
    private readonly ILogger<NoOpObservabilityService> _logger;

    public NoOpObservabilityService(ILogger<NoOpObservabilityService> logger)
    {
        _logger = logger;
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        _logger.LogInformation("Event: {EventName} with properties: {Properties}", 
            eventName, properties != null ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")) : "none");
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        _logger.LogError(exception, "Exception tracked: {ExceptionType} with properties: {Properties}", 
            exception.GetType().Name, properties != null ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")) : "none");
    }

    public void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        _logger.LogDebug("Metric: {MetricName} = {Value} with properties: {Properties}", 
            metricName, value, properties != null ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")) : "none");
    }

    public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
    {
        _logger.LogDebug("Dependency: {DependencyName}.{CommandName} - Success: {Success}, Duration: {Duration}ms", 
            dependencyName, commandName, success, duration.TotalMilliseconds);
    }

    public void TrackPageView(string pageName, Dictionary<string, string>? properties = null)
    {
        _logger.LogDebug("Page view: {PageName} with properties: {Properties}", 
            pageName, properties != null ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")) : "none");
    }

    public void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
    {
        _logger.LogDebug("Request: {RequestName} - {ResponseCode}, Duration: {Duration}ms", 
            name, responseCode, duration.TotalMilliseconds);
    }

    public IDisposable StartOperation(string operationName, Dictionary<string, string>? properties = null)
    {
        _logger.LogDebug("Operation started: {OperationName} with properties: {Properties}", 
            operationName, properties != null ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")) : "none");
        return new NoOpDisposable();
    }

    public void SetUser(string userId, string? accountId = null)
    {
        _logger.LogDebug("User context set: {UserId}, Account: {AccountId}", userId, accountId ?? "none");
    }

    public void AddGlobalProperty(string key, string value)
    {
        _logger.LogDebug("Global property added: {Key} = {Value}", key, value);
    }

    public Task FlushAsync()
    {
        _logger.LogDebug("Telemetry flush requested (no-op)");
        return Task.CompletedTask;
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}