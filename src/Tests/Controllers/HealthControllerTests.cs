using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Controllers;
using PortfolioCMS.API.Services;

namespace PortfolioCMS.Tests.Controllers;

public class HealthControllerTests
{
    private readonly Mock<IObservabilityService> _mockObservabilityService;
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockObservabilityService = new Mock<IObservabilityService>();
        _mockLogger = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_mockObservabilityService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetHealth_WhenAllServicesHealthy_ShouldReturnOkWithHealthyStatus()
    {
        // Arrange
        var healthStatus = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Database = "Healthy",
            Dependencies = new { }
        };

        _mockObservabilityService.Setup(s => s.GetHealthStatusAsync())
            .ReturnsAsync(healthStatus);

        // Act
        var result = await _controller.GetHealth();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStatus = okResult.Value.Should().BeAssignableTo<object>().Subject;
    }

    [Fact]
    public async Task GetHealth_WhenDatabaseUnhealthy_ShouldReturnServiceUnavailable()
    {
        // Arrange
        var healthStatus = new
        {
            Status = "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Database = "Unhealthy",
            Dependencies = new { }
        };

        _mockObservabilityService.Setup(s => s.GetHealthStatusAsync())
            .ReturnsAsync(healthStatus);

        // Act
        var result = await _controller.GetHealth();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(503); // Service Unavailable
    }

    [Fact]
    public async Task GetReadiness_WhenSystemReady_ShouldReturnOk()
    {
        // Arrange
        var readinessStatus = new
        {
            Status = "Ready",
            Timestamp = DateTime.UtcNow,
            Database = "Connected",
            Migrations = "Applied"
        };

        _mockObservabilityService.Setup(s => s.GetReadinessStatusAsync())
            .ReturnsAsync(readinessStatus);

        // Act
        var result = await _controller.GetReadiness();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStatus = okResult.Value.Should().BeAssignableTo<object>().Subject;
    }

    [Fact]
    public async Task GetReadiness_WhenSystemNotReady_ShouldReturnServiceUnavailable()
    {
        // Arrange
        var readinessStatus = new
        {
            Status = "NotReady",
            Timestamp = DateTime.UtcNow,
            Database = "Disconnected",
            Migrations = "Pending"
        };

        _mockObservabilityService.Setup(s => s.GetReadinessStatusAsync())
            .ReturnsAsync(readinessStatus);

        // Act
        var result = await _controller.GetReadiness();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(503); // Service Unavailable
    }

    [Fact]
    public async Task GetLiveness_ShouldAlwaysReturnOk()
    {
        // Arrange
        var livenessStatus = new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            Uptime = TimeSpan.FromMinutes(30)
        };

        _mockObservabilityService.Setup(s => s.GetLivenessStatusAsync())
            .ReturnsAsync(livenessStatus);

        // Act
        var result = await _controller.GetLiveness();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStatus = okResult.Value.Should().BeAssignableTo<object>().Subject;
    }

    [Fact]
    public async Task GetMetrics_ShouldReturnOkWithMetrics()
    {
        // Arrange
        var metrics = new
        {
            RequestCount = 1000,
            AverageResponseTime = 150.5,
            ErrorRate = 0.02,
            ActiveConnections = 25,
            MemoryUsage = 512.5,
            CpuUsage = 45.2
        };

        _mockObservabilityService.Setup(s => s.GetMetricsAsync())
            .ReturnsAsync(metrics);

        // Act
        var result = await _controller.GetMetrics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedMetrics = okResult.Value.Should().BeAssignableTo<object>().Subject;
    }
}