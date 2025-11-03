using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Entities;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.Tests.Services;

public class ContentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IArticleRepository> _mockArticleRepository;
    private readonly Mock<ILogger<ContentService>> _mockLogger;
    private readonly ContentService _contentService;

    public ContentServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockArticleRepository = new Mock<IArticleRepository>();
        _mockLogger = new Mock<ILogger<ContentService>>();
        
        _mockUnitOfWork.Setup(u => u.Articles).Returns(_mockArticleRepository.Object);
        
        _contentService = new ContentService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetPublishedArticlesAsync_ShouldReturnOnlyPublishedArticles()
    {
        // Arrange
        var publishedArticles = new List<Article>
        {
            new() { Id = 1, Title = "Published 1", Status = ArticleStatus.Published, PublishedDate = DateTime.UtcNow },
            new() { Id = 2, Title = "Published 2", Status = ArticleStatus.Published, PublishedDate = DateTime.UtcNow }
        };

        _mockArticleRepository.Setup(r => r.GetPublishedArticlesAsync())
            .ReturnsAsync(publishedArticles);

        // Act
        var result = await _contentService.GetPublishedArticlesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.Status == ArticleStatus.Published);
        _mockArticleRepository.Verify(r => r.GetPublishedArticlesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetArticleByIdAsync_WhenArticleExists_ShouldReturnArticle()
    {
        // Arrange
        var articleId = 1;
        var expectedArticle = new Article 
        { 
            Id = articleId, 
            Title = "Test Article", 
            Status = ArticleStatus.Published 
        };

        _mockArticleRepository.Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync(expectedArticle);

        // Act
        var result = await _contentService.GetArticleByIdAsync(articleId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(articleId);
        result.Title.Should().Be("Test Article");
    }

    [Fact]
    public async Task GetArticleByIdAsync_WhenArticleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var articleId = 999;
        _mockArticleRepository.Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync((Article?)null);

        // Act
        var result = await _contentService.GetArticleByIdAsync(articleId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateArticleAsync_WithValidData_ShouldCreateAndReturnArticle()
    {
        // Arrange
        var newArticle = new Article
        {
            Title = "New Article",
            Content = "Article content",
            Summary = "Article summary",
            Status = ArticleStatus.Draft
        };

        _mockArticleRepository.Setup(r => r.AddAsync(It.IsAny<Article>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _contentService.CreateArticleAsync(newArticle);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Article");
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockArticleRepository.Verify(r => r.AddAsync(It.IsAny<Article>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateArticleAsync_WithValidData_ShouldUpdateAndReturnArticle()
    {
        // Arrange
        var existingArticle = new Article
        {
            Id = 1,
            Title = "Original Title",
            Content = "Original content",
            CreatedDate = DateTime.UtcNow.AddDays(-1)
        };

        var updatedArticle = new Article
        {
            Id = 1,
            Title = "Updated Title",
            Content = "Updated content"
        };

        _mockArticleRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingArticle);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _contentService.UpdateArticleAsync(updatedArticle);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Content.Should().Be("Updated content");
        result.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteArticleAsync_WhenArticleExists_ShouldDeleteArticle()
    {
        // Arrange
        var articleId = 1;
        var existingArticle = new Article { Id = articleId, Title = "To Delete" };

        _mockArticleRepository.Setup(r => r.GetByIdAsync(articleId))
            .ReturnsAsync(existingArticle);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _contentService.DeleteArticleAsync(articleId);

        // Assert
        result.Should().BeTrue();
        _mockArticleRepository.Verify(r => r.Delete(existingArticle), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchArticlesAsync_WithValidQuery_ShouldReturnMatchingArticles()
    {
        // Arrange
        var searchQuery = "test";
        var matchingArticles = new List<Article>
        {
            new() { Id = 1, Title = "Test Article 1", Status = ArticleStatus.Published },
            new() { Id = 2, Title = "Another Test", Status = ArticleStatus.Published }
        };

        _mockArticleRepository.Setup(r => r.SearchAsync(searchQuery))
            .ReturnsAsync(matchingArticles);

        // Act
        var result = await _contentService.SearchArticlesAsync(searchQuery);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.Title.Contains("Test", StringComparison.OrdinalIgnoreCase));
    }
}