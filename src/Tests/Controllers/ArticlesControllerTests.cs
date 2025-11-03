using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Controllers;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.Tests.Controllers;

public class ArticlesControllerTests
{
    private readonly Mock<IContentService> _mockContentService;
    private readonly Mock<ILogger<ArticlesController>> _mockLogger;
    private readonly ArticlesController _controller;

    public ArticlesControllerTests()
    {
        _mockContentService = new Mock<IContentService>();
        _mockLogger = new Mock<ILogger<ArticlesController>>();
        _controller = new ArticlesController(_mockContentService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetPublishedArticles_ShouldReturnOkWithArticles()
    {
        // Arrange
        var articles = new List<Article>
        {
            new() { Id = 1, Title = "Article 1", Status = ArticleStatus.Published },
            new() { Id = 2, Title = "Article 2", Status = ArticleStatus.Published }
        };

        _mockContentService.Setup(s => s.GetPublishedArticlesAsync())
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetPublishedArticles();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticles = okResult.Value.Should().BeAssignableTo<IEnumerable<ArticleDto>>().Subject;
        returnedArticles.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetArticleById_WhenArticleExists_ShouldReturnOkWithArticle()
    {
        // Arrange
        var articleId = 1;
        var article = new Article
        {
            Id = articleId,
            Title = "Test Article",
            Content = "Test content",
            Status = ArticleStatus.Published
        };

        _mockContentService.Setup(s => s.GetArticleByIdAsync(articleId))
            .ReturnsAsync(article);

        // Act
        var result = await _controller.GetArticleById(articleId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticle = okResult.Value.Should().BeOfType<ArticleDto>().Subject;
        returnedArticle.Id.Should().Be(articleId);
        returnedArticle.Title.Should().Be("Test Article");
    }

    [Fact]
    public async Task GetArticleById_WhenArticleDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var articleId = 999;
        _mockContentService.Setup(s => s.GetArticleByIdAsync(articleId))
            .ReturnsAsync((Article?)null);

        // Act
        var result = await _controller.GetArticleById(articleId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateArticle_WithValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateArticleDto
        {
            Title = "New Article",
            Content = "New content",
            Summary = "New summary"
        };

        var createdArticle = new Article
        {
            Id = 1,
            Title = createDto.Title,
            Content = createDto.Content,
            Summary = createDto.Summary,
            Status = ArticleStatus.Draft
        };

        _mockContentService.Setup(s => s.CreateArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync(createdArticle);

        // Act
        var result = await _controller.CreateArticle(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedArticle = createdResult.Value.Should().BeOfType<ArticleDto>().Subject;
        returnedArticle.Title.Should().Be("New Article");
        createdResult.ActionName.Should().Be(nameof(ArticlesController.GetArticleById));
    }

    [Fact]
    public async Task UpdateArticle_WithValidData_ShouldReturnOkWithUpdatedArticle()
    {
        // Arrange
        var articleId = 1;
        var updateDto = new UpdateArticleDto
        {
            Title = "Updated Article",
            Content = "Updated content",
            Summary = "Updated summary"
        };

        var updatedArticle = new Article
        {
            Id = articleId,
            Title = updateDto.Title,
            Content = updateDto.Content,
            Summary = updateDto.Summary
        };

        _mockContentService.Setup(s => s.UpdateArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync(updatedArticle);

        // Act
        var result = await _controller.UpdateArticle(articleId, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticle = okResult.Value.Should().BeOfType<ArticleDto>().Subject;
        returnedArticle.Title.Should().Be("Updated Article");
    }

    [Fact]
    public async Task DeleteArticle_WhenArticleExists_ShouldReturnNoContent()
    {
        // Arrange
        var articleId = 1;
        _mockContentService.Setup(s => s.DeleteArticleAsync(articleId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteArticle(articleId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteArticle_WhenArticleDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var articleId = 999;
        _mockContentService.Setup(s => s.DeleteArticleAsync(articleId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteArticle(articleId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SearchArticles_WithValidQuery_ShouldReturnOkWithResults()
    {
        // Arrange
        var searchQuery = "test";
        var searchResults = new List<Article>
        {
            new() { Id = 1, Title = "Test Article", Status = ArticleStatus.Published },
            new() { Id = 2, Title = "Another Test", Status = ArticleStatus.Published }
        };

        _mockContentService.Setup(s => s.SearchArticlesAsync(searchQuery))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.SearchArticles(searchQuery);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticles = okResult.Value.Should().BeAssignableTo<IEnumerable<ArticleDto>>().Subject;
        returnedArticles.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchArticles_WithInvalidQuery_ShouldReturnBadRequest(string invalidQuery)
    {
        // Act
        var result = await _controller.SearchArticles(invalidQuery);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}