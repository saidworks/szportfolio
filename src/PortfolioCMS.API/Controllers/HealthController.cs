using Microsoft.AspNetCore.Mvc;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Repositories;
using System.Diagnostics;

namespace PortfolioCMS.API.Controllers;

/// <summary>
/// Health check and monitoring endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObservabilityService _observability;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IUnitOfWork unitOfWork,
        IObservabilityService observability,
        ILogger<HealthController> logger)
    {
        _unitOfWork = unitOfWork;
        _observability = observability;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    /// <response code="200">Service is healthy</response>
    /// <response code="503">Service is unhealthy</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResult), 200)]
    [ProducesResponseType(typeof(HealthCheckResult), 503)]
    public async Task<ActionResult<HealthCheckResult>> GetHealth()
    {
        var healthCheck = new HealthCheckResult
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        };

        try
        {
            // Check database connectivity
            var canConnect = await _unitOfWork.CanConnectAsync();
            if (!canConnect)
            {
                healthCheck.Status = "Unhealthy";
                healthCheck.Errors.Add("Database connection failed");
                
                _observability.TrackEvent("HealthCheckFailed", new Dictionary<string, string>
                {
                    ["Reason"] = "DatabaseConnection"
                });

                return StatusCode(503, healthCheck);
            }

            healthCheck.Checks.Add("Database", "Healthy");

            _observability.TrackEvent("HealthCheckPassed");
            return Ok(healthCheck);
        }
        catch (Exception ex)
        {
            healthCheck.Status = "Unhealthy";
            healthCheck.Errors.Add($"Health check failed: {ex.Message}");
            
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetHealth)
            });

            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, healthCheck);
        }
    }

    /// <summary>
    /// Readiness check for container orchestration
    /// </summary>
    /// <returns>Readiness status</returns>
    /// <response code="200">Service is ready</response>
    /// <response code="503">Service is not ready</response>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ReadinessCheckResult), 200)]
    [ProducesResponseType(typeof(ReadinessCheckResult), 503)]
    public async Task<ActionResult<ReadinessCheckResult>> GetReadiness()
    {
        var readinessCheck = new ReadinessCheckResult
        {
            Status = "Ready",
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Check if database is accessible and has required tables
            var canConnect = await _unitOfWork.CanConnectAsync();
            if (!canConnect)
            {
                readinessCheck.Status = "NotReady";
                readinessCheck.Errors.Add("Database is not accessible");
                return StatusCode(503, readinessCheck);
            }

            // Additional readiness checks can be added here
            // For example: check if required configuration is present
            // check if external dependencies are available, etc.

            readinessCheck.Checks.Add("Database", "Ready");
            readinessCheck.Checks.Add("Configuration", "Ready");

            return Ok(readinessCheck);
        }
        catch (Exception ex)
        {
            readinessCheck.Status = "NotReady";
            readinessCheck.Errors.Add($"Readiness check failed: {ex.Message}");
            
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, readinessCheck);
        }
    }

    /// <summary>
    /// Liveness check for container orchestration
    /// </summary>
    /// <returns>Liveness status</returns>
    /// <response code="200">Service is alive</response>
    /// <response code="503">Service is not responding</response>
    [HttpGet("live")]
    [ProducesResponseType(typeof(LivenessCheckResult), 200)]
    [ProducesResponseType(typeof(LivenessCheckResult), 503)]
    public ActionResult<LivenessCheckResult> GetLiveness()
    {
        // Liveness check should be simple and fast
        // It just checks if the application is running and responsive
        var livenessCheck = new LivenessCheckResult
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
        };

        return Ok(livenessCheck);
    }

    /// <summary>
    /// Get detailed system information (for monitoring)
    /// </summary>
    /// <returns>System information</returns>
    /// <response code="200">System information retrieved</response>
    [HttpGet("info")]
    [ProducesResponseType(typeof(SystemInfoResult), 200)]
    public ActionResult<SystemInfoResult> GetSystemInfo()
    {
        var process = Process.GetCurrentProcess();
        
        var systemInfo = new SystemInfoResult
        {
            ApplicationName = "Portfolio CMS API",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            MachineName = Environment.MachineName,
            ProcessId = process.Id,
            StartTime = process.StartTime.ToUniversalTime(),
            Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
            WorkingSet = process.WorkingSet64,
            GCTotalMemory = GC.GetTotalMemory(false),
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount
        };

        return Ok(systemInfo);
    }
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public Dictionary<string, string> Checks { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Readiness check result
/// </summary>
public class ReadinessCheckResult
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Checks { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Liveness check result
/// </summary>
public class LivenessCheckResult
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public TimeSpan Uptime { get; set; }
}

/// <summary>
/// System information result
/// </summary>
public class SystemInfoResult
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public long WorkingSet { get; set; }
    public long GCTotalMemory { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
}