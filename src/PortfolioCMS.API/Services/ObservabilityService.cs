using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace PortfolioCMS.API.Services;

public class ObservabilityService : IObservabilityService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ObservabilityService> _logger;

    public ObservabilityService(TelemetryClient telemetryClient, ILogger<ObservabilityService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        try
        {
            _telemetryClient.TrackEvent(eventName, properties);
            _logger.LogInformation("Event tracked: {EventName}", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track event: {EventName}", eventName);
        }
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        try
        {
            var telemetry = new ExceptionTelemetry(exception);
            
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    telemetry.Properties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackException(telemetry);
            _logger.LogError(exception, "Exception tracked: {ExceptionType}", exception.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track exception: {OriginalException}", exception.Message);
        }
    }

    public void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        try
        {
            _telemetryClient.TrackMetric(metricName, value, properties);
            _logger.LogDebug("Metric tracked: {MetricName} = {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track metric: {MetricName}", metricName);
        }
    }

    public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
    {
        try
        {
            var telemetry = new DependencyTelemetry(dependencyName, commandName, commandName, commandName)
            {
                Timestamp = startTime,
                Duration = duration,
                Success = success
            };
            
            _telemetryClient.TrackDependency(telemetry);
            _logger.LogDebug("Dependency tracked: {DependencyName}.{CommandName} - Success: {Success}, Duration: {Duration}ms", 
                dependencyName, commandName, success, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track dependency: {DependencyName}.{CommandName}", dependencyName, commandName);
        }
    }

    public void TrackPageView(string pageName, Dictionary<string, string>? properties = null)
    {
        try
        {
            var telemetry = new PageViewTelemetry(pageName);
            
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    telemetry.Properties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackPageView(telemetry);
            _logger.LogDebug("Page view tracked: {PageName}", pageName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track page view: {PageName}", pageName);
        }
    }

    public void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
    {
        try
        {
            _telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
            _logger.LogDebug("Request tracked: {RequestName} - {ResponseCode}, Duration: {Duration}ms", 
                name, responseCode, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track request: {RequestName}", name);
        }
    }

    public IDisposable StartOperation(string operationName, Dictionary<string, string>? properties = null)
    {
        try
        {
            var operation = _telemetryClient.StartOperation<RequestTelemetry>(operationName);
            
            if (properties != null && operation.Telemetry != null)
            {
                foreach (var property in properties)
                {
                    operation.Telemetry.Properties[property.Key] = property.Value;
                }
            }

            _logger.LogDebug("Operation started: {OperationName}", operationName);
            return operation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start operation: {OperationName}", operationName);
            return new NoOpDisposable();
        }
    }

    public void SetUser(string userId, string? accountId = null)
    {
        try
        {
            _telemetryClient.Context.User.Id = userId;
            if (!string.IsNullOrEmpty(accountId))
            {
                _telemetryClient.Context.User.AccountId = accountId;
            }
            _logger.LogDebug("User context set: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set user context: {UserId}", userId);
        }
    }

    public void AddGlobalProperty(string key, string value)
    {
        try
        {
            _telemetryClient.Context.GlobalProperties[key] = value;
            _logger.LogDebug("Global property added: {Key} = {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add global property: {Key}", key);
        }
    }

    public async Task FlushAsync()
    {
        try
        {
            _telemetryClient.Flush();
            // Wait a bit to allow the flush to complete
            await Task.Delay(1000);
            _logger.LogDebug("Telemetry flushed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush telemetry");
        }
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}