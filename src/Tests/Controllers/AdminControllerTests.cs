using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Controllers;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IContentService> _mockContentService;
    private readonly Mock<ICommentService> _mockCommentService;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<ILogger<AdminController>> _mockLogger;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mockContentService = new Mock<IContentService>();
        _mockCommentService = new Mock<ICommentService>();
        _mockMediaService = new Mock<IMediaService>();
        _mockLogger = new Mock<ILogger<AdminController>>();
        
        _controller = new AdminController(
            _mockContentService.Object,
            _mockCommentService.Object,
            _mockMediaService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllArticles_ShouldReturnOkWithAllArticles()
    {
        // Arrange
        var articles = new List<Article>
        {
            new() { Id = 1, Title = "Draft Article", Status = ArticleStatus.Draft },
            new() { Id = 2, Title = "Published Article", Status = ArticleStatus.Published }
        };

        _mockContentService.Setup(s => s.GetAllArticlesAsync())
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetAllArticles();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticles = okResult.Value.Should().BeAssignableTo<IEnumerable<ArticleDto>>().Subject;
        returnedArticles.Should().HaveCount(2);
    }

    [Fact]
    public async Task PublishArticle_WhenArticleExists_ShouldReturnOk()
    {
        // Arrange
        var articleId = 1;
        var publishedArticle = new Article
        {
            Id = articleId,
            Title = "Published Article",
            Status = ArticleStatus.Published,
            PublishedDate = DateTime.UtcNow
        };

        _mockContentService.Setup(s => s.PublishArticleAsync(articleId))
            .ReturnsAsync(publishedArticle);

        // Act
        var result = await _controller.PublishArticle(articleId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticle = okResult.Value.Should().BeOfType<ArticleDto>().Subject;
        returnedArticle.Status.Should().Be(ArticleStatus.Published);
    }

    [Fact]
    public async Task PublishArticle_WhenArticleDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var articleId = 999;
        _mockContentService.Setup(s => s.PublishArticleAsync(articleId))
            .ReturnsAsync((Article?)null);

        // Act
        var result = await _controller.PublishArticle(articleId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UnpublishArticle_WhenArticleExists_ShouldReturnOk()
    {
        // Arrange
        var articleId = 1;
        var unpublishedArticle = new Article
        {
            Id = articleId,
            Title = "Draft Article",
            Status = ArticleStatus.Draft,
            PublishedDate = null
        };

        _mockContentService.Setup(s => s.UnpublishArticleAsync(articleId))
            .ReturnsAsync(unpublishedArticle);

        // Act
        var result = await _controller.UnpublishArticle(articleId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticle = okResult.Value.Should().BeOfType<ArticleDto>().Subject;
        returnedArticle.Status.Should().Be(ArticleStatus.Draft);
    }

    [Fact]
    public async Task GetDashboardStats_ShouldReturnOkWithStats()
    {
        // Arrange
        var stats = new DashboardStatsDto
        {
            TotalArticles = 10,
            PublishedArticles = 7,
            DraftArticles = 3,
            PendingComments = 5,
            TotalComments = 25
        };

        _mockContentService.Setup(s => s.GetDashboardStatsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetDashboardStats();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<DashboardStatsDto>().Subject;
        returnedStats.TotalArticles.Should().Be(10);
        returnedStats.PublishedArticles.Should().Be(7);
        returnedStats.PendingComments.Should().Be(5);
    }

    [Fact]
    public async Task BulkApproveComments_WithValidIds_ShouldReturnOk()
    {
        // Arrange
        var commentIds = new List<int> { 1, 2, 3 };
        var bulkRequest = new BulkCommentActionDto { CommentIds = commentIds };

        _mockCommentService.Setup(s => s.BulkApproveCommentsAsync(commentIds))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkApproveComments(bulkRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
    }

    [Fact]
    public async Task BulkRejectComments_WithValidIds_ShouldReturnOk()
    {
        // Arrange
        var commentIds = new List<int> { 1, 2, 3 };
        var bulkRequest = new BulkCommentActionDto { CommentIds = commentIds };

        _mockCommentService.Setup(s => s.BulkRejectCommentsAsync(commentIds))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkRejectComments(bulkRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
    }

    [Fact]
    public async Task BulkDeleteComments_WithValidIds_ShouldReturnOk()
    {
        // Arrange
        var commentIds = new List<int> { 1, 2, 3 };
        var bulkRequest = new BulkCommentActionDto { CommentIds = commentIds };

        _mockCommentService.Setup(s => s.BulkDeleteCommentsAsync(commentIds))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDeleteComments(bulkRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
    }

    [Fact]
    public async Task GetMediaFiles_ShouldReturnOkWithMediaFiles()
    {
        // Arrange
        var mediaFiles = new List<MediaFile>
        {
            new() { Id = 1, OriginalFileName = "image1.jpg", ContentType = "image/jpeg" },
            new() { Id = 2, OriginalFileName = "image2.png", ContentType = "image/png" }
        };

        _mockMediaService.Setup(s => s.GetAllMediaFilesAsync())
            .ReturnsAsync(mediaFiles);

        // Act
        var result = await _controller.GetMediaFiles();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFiles = okResult.Value.Should().BeAssignableTo<IEnumerable<MediaFileDto>>().Subject;
        returnedFiles.Should().HaveCount(2);
    }
}