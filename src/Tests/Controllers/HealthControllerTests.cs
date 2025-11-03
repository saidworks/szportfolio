using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Controllers;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Repositories;
using FluentAssertions;

namespace PortfolioCMS.Tests.Controllers;

public class HealthControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IObservabilityService> _mockObservability;
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockObservability = new Mock<IObservabilityService>();
        _mockLogger = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_mockUnitOfWork.Object, _mockObservability.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetHealth_ReturnsOkResult()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.CanConnectAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.GetHealth();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReadiness_ReturnsOkResult()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.CanConnectAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.GetReadiness();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetLiveness_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetLiveness();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
}