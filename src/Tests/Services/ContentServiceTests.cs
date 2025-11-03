using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Repositories;
using FluentAssertions;

namespace PortfolioCMS.Tests.Services;

public class ContentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IObservabilityService> _mockObservability;
    private readonly Mock<ILogger<ContentService>> _mockLogger;
    private readonly ContentService _service;

    public ContentServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockObservability = new Mock<IObservabilityService>();
        _mockLogger = new Mock<ILogger<ContentService>>();
        _service = new ContentService(_mockUnitOfWork.Object, _mockObservability.Object, _mockLogger.Object);
    }

    [Fact]
    public void ContentService_ShouldBeCreated()
    {
        // Assert
        _service.Should().NotBeNull();
    }
}