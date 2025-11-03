using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Context;

namespace PortfolioCMS.Tests.Services;

public class ObservabilityServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<ObservabilityService>> _mockLogger;
    private readonly ObservabilityService _observabilityService;

    public ObservabilityServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockLogger = new Mock<ILogger<ObservabilityService>>();
        _observabilityService = new ObservabilityService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetHealthStatusAsync_WhenDatabaseConnected_ShouldReturnHealthyStatus()
    {
        // Arrange
        _mockDbContext.Setup(db => db.Database.CanConnectAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _observabilityService.GetHealthStatusAsync();

        // Assert
        result.Should().NotBeNull();
        var status = result.GetType().GetProperty("Status")?.GetValue(result)?.ToString();
        status.Should().Be("Healthy");
        
        var database = result.GetType().GetProperty("Database")?.GetValue(result)?.ToString();
        database.Should().Be("Healthy");
    }

    [Fact]
    public async Task GetHealthStatusAsync_WhenDatabaseDisconnected_ShouldReturnUnhealthyStatus()
    {
        // Arrange
        _mockDbContext.Setup(db => db.Database.CanConnectAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _observabilityService.GetHealthStatusAsync();

        // Assert
        result.Should().NotBeNull();
        var status = result.GetType().GetProperty("Status")?.GetValue(result)?.ToString();
        status.Should().Be("Unhealthy");
        
        var database = result.GetType().GetProperty("Database")?.GetValue(result)?.ToString();
        database.Should().Be("Unhealthy");
    }

    [Fact]
    public async Task GetReadinessStatusAsync_WhenDatabaseReadyAndMigrationsApplied_ShouldReturnReady()
    {
        // Arrange
        _mockDbContext.Setup(db => db.Database.CanConnectAsync())
            .ReturnsAsync(true);
        _mockDbContext.Setup(db => db.Database.GetPendingMigrationsAsync())
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _observabilityService.GetReadinessStatusAsync();

        // Assert
        result.Should().NotBeNull();
        var status = result.GetType().GetProperty("Status")?.GetValue(result)?.ToString();
        status.Should().Be("Ready");
        
        var database = result.GetType().GetProperty("Database")?.GetValue(result)?.ToString();
        database.Should().Be("Connected");
        
        var migrations = result.GetType().GetProperty("Migrations")?.GetValue(result)?.ToString();
        migrations.Should().Be("Applied");
    }

    [Fact]
    public async Task GetReadinessStatusAsync_WhenMigrationsPending_ShouldReturnNotReady()
    {
        // Arrange
        _mockDbContext.Setup(db => db.Database.CanConnectAsync())
            .ReturnsAsync(true);
        _mockDbContext.Setup(db => db.Database.GetPendingMigrationsAsync())
            .ReturnsAsync(new List<string> { "20231101_InitialMigration" });

        // Act
        var result = await _observabilityService.GetReadinessStatusAsync();

        // Assert
        result.Should().NotBeNull();
        var status = result.GetType().GetProperty("Status")?.GetValue(result)?.ToString();
        status.Should().Be("NotReady");
        
        var migrations = result.GetType().GetProperty("Migrations")?.GetValue(result)?.ToString();
        migrations.Should().Be("Pending");
    }

    [Fact]
    public async Task GetLivenessStatusAsync_ShouldAlwaysReturnAlive()
    {
        // Act
        var result = await _observabilityService.GetLivenessStatusAsync();

        // Assert
        result.Should().NotBeNull();
        var status = result.GetType().GetProperty("Status")?.GetValue(result)?.ToString();
        status.Should().Be("Alive");
        
        var timestamp = result.GetType().GetProperty("Timestamp")?.GetValue(result);
        timestamp.Should().NotBeNull();
        timestamp.Should().BeOfType<DateTime>();
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldReturnSystemMetrics()
    {
        // Act
        var result = await _observabilityService.GetMetricsAsync();

        // Assert
        result.Should().NotBeNull();
        
        // Verify that metrics properties exist
        var requestCount = result.GetType().GetProperty("RequestCount")?.GetValue(result);
        requestCount.Should().NotBeNull();
        
        var memoryUsage = result.GetType().GetProperty("MemoryUsage")?.GetValue(result);
        memoryUsage.Should().NotBeNull();
        
        var cpuUsage = result.GetType().GetProperty("CpuUsage")?.GetValue(result);
        cpuUsage.Should().NotBeNull();
    }

    [Fact]
    public void LogCustomEvent_ShouldLogEventWithProperties()
    {
        // Arrange
        var eventName = "ArticleCreated";
        var properties = new Dictionary<string, object>
        {
            { "ArticleId", 1 },
            { "Title", "Test Article" },
            { "Author", "Test Author" }
        };

        // Act
        _observabilityService.LogCustomEvent(eventName, properties);

        // Assert
        // Verify that the logger was called (in a real implementation, this would log to Application Insights)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(eventName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCustomMetric_ShouldLogMetricWithValue()
    {
        // Arrange
        var metricName = "ArticleViewCount";
        var value = 150.5;
        var properties = new Dictionary<string, string>
        {
            { "ArticleId", "1" },
            { "Category", "Technology" }
        };

        // Act
        _observabilityService.LogCustomMetric(metricName, value, properties);

        // Assert
        // Verify that the logger was called (in a real implementation, this would log to Application Insights)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(metricName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void TrackDependency_ShouldLogDependencyCall()
    {
        // Arrange
        var dependencyName = "AzureSQL";
        var commandName = "GetArticles";
        var startTime = DateTime.UtcNow.AddSeconds(-2);
        var duration = TimeSpan.FromSeconds(2);
        var success = true;

        // Act
        _observabilityService.TrackDependency(dependencyName, commandName, startTime, duration, success);

        // Assert
        // Verify that the logger was called (in a real implementation, this would log to Application Insights)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(dependencyName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}