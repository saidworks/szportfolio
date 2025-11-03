using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Entities;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.Tests.Services;

public class MediaServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<MediaFile>> _mockMediaRepository;
    private readonly Mock<ILogger<MediaService>> _mockLogger;
    private readonly MediaService _mediaService;

    public MediaServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMediaRepository = new Mock<IRepository<MediaFile>>();
        _mockLogger = new Mock<ILogger<MediaService>>();
        
        _mockUnitOfWork.Setup(u => u.GetRepository<MediaFile>()).Returns(_mockMediaRepository.Object);
        
        _mediaService = new MediaService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UploadFileAsync_WithValidImage_ShouldCreateMediaFile()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileName = "test-image.jpg";
        var fileContent = "fake image content"u8.ToArray();
        var stream = new MemoryStream(fileContent);

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        _mockMediaRepository.Setup(r => r.AddAsync(It.IsAny<MediaFile>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _mediaService.UploadFileAsync(mockFile.Object);

        // Assert
        result.Should().NotBeNull();
        result.OriginalFileName.Should().Be(fileName);
        result.ContentType.Should().Be("image/jpeg");
        result.FileSize.Should().Be(fileContent.Length);
        result.UploadDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockMediaRepository.Verify(r => r.AddAsync(It.IsAny<MediaFile>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WithInvalidFileType_ShouldThrowArgumentException()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.exe");
        mockFile.Setup(f => f.ContentType).Returns("application/x-msdownload");
        mockFile.Setup(f => f.Length).Returns(1000);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _mediaService.UploadFileAsync(mockFile.Object));
    }

    [Fact]
    public async Task UploadFileAsync_WithFileTooLarge_ShouldThrowArgumentException()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("large-image.jpg");
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.Length).Returns(10 * 1024 * 1024 + 1); // 10MB + 1 byte

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _mediaService.UploadFileAsync(mockFile.Object));
    }

    [Fact]
    public async Task GetMediaFileAsync_WhenFileExists_ShouldReturnMediaFile()
    {
        // Arrange
        var fileId = 1;
        var expectedFile = new MediaFile
        {
            Id = fileId,
            OriginalFileName = "test.jpg",
            StoredFileName = "stored-test.jpg",
            ContentType = "image/jpeg"
        };

        _mockMediaRepository.Setup(r => r.GetByIdAsync(fileId))
            .ReturnsAsync(expectedFile);

        // Act
        var result = await _mediaService.GetMediaFileAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(fileId);
        result.OriginalFileName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task GetAllMediaFilesAsync_ShouldReturnAllFiles()
    {
        // Arrange
        var mediaFiles = new List<MediaFile>
        {
            new() { Id = 1, OriginalFileName = "file1.jpg", ContentType = "image/jpeg" },
            new() { Id = 2, OriginalFileName = "file2.png", ContentType = "image/png" }
        };

        _mockMediaRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(mediaFiles);

        // Act
        var result = await _mediaService.GetAllMediaFilesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.OriginalFileName == "file1.jpg");
        result.Should().Contain(f => f.OriginalFileName == "file2.png");
    }

    [Fact]
    public async Task DeleteMediaFileAsync_WhenFileExists_ShouldDeleteFile()
    {
        // Arrange
        var fileId = 1;
        var existingFile = new MediaFile
        {
            Id = fileId,
            OriginalFileName = "to-delete.jpg",
            StoredFileName = "stored-to-delete.jpg"
        };

        _mockMediaRepository.Setup(r => r.GetByIdAsync(fileId))
            .ReturnsAsync(existingFile);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _mediaService.DeleteMediaFileAsync(fileId);

        // Assert
        result.Should().BeTrue();
        _mockMediaRepository.Verify(r => r.Delete(existingFile), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public void IsValidImageType_WithValidImageTypes_ShouldReturnTrue(string contentType)
    {
        // Act
        var result = _mediaService.IsValidImageType(contentType);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("application/x-msdownload")]
    [InlineData("video/mp4")]
    public void IsValidImageType_WithInvalidTypes_ShouldReturnFalse(string contentType)
    {
        // Act
        var result = _mediaService.IsValidImageType(contentType);

        // Assert
        result.Should().BeFalse();
    }
}