using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortfolioCMS.API.Controllers;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.Tests.Integration;

public class ArticleIntegrationTests : IClassFixture<TestWebApplicationFactory<ArticlesController>>
{
    private readonly TestWebApplicationFactory<ArticlesController> _factory;

    public ArticleIntegrationTests(TestWebApplicationFactory<ArticlesController> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Article_ShouldHaveRequiredProperties()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var article = new Article
        {
            Title = "Test Article",
            Content = "This is test content",
            Summary = "Test summary",
            CreatedDate = DateTime.UtcNow,
            Status = ArticleStatus.Draft
        };

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Assert
        var savedArticle = await context.Articles.FirstOrDefaultAsync(a => a.Title == "Test Article");
        savedArticle.Should().NotBeNull();
        savedArticle!.Title.Should().Be("Test Article");
        savedArticle.Content.Should().Be("This is test content");
        savedArticle.Summary.Should().Be("Test summary");
        savedArticle.Status.Should().Be(ArticleStatus.Draft);
        savedArticle.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Article_ShouldSupportPublishedStatus()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var article = new Article
        {
            Title = "Published Article",
            Content = "Published content",
            Summary = "Published summary",
            CreatedDate = DateTime.UtcNow,
            PublishedDate = DateTime.UtcNow,
            Status = ArticleStatus.Published
        };

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Assert
        var savedArticle = await context.Articles.FirstOrDefaultAsync(a => a.Title == "Published Article");
        savedArticle.Should().NotBeNull();
        savedArticle!.Status.Should().Be(ArticleStatus.Published);
        savedArticle.PublishedDate.Should().NotBeNull();
        savedArticle.PublishedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Article_ShouldSupportTagsRelationship()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var tag1 = new Tag { Name = "C#", Slug = "csharp" };
        var tag2 = new Tag { Name = "ASP.NET", Slug = "aspnet" };
        
        var article = new Article
        {
            Title = "Article with Tags",
            Content = "Content with tags",
            Summary = "Summary",
            CreatedDate = DateTime.UtcNow,
            Status = ArticleStatus.Draft,
            Tags = new List<Tag> { tag1, tag2 }
        };

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Assert
        var savedArticle = await context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Title == "Article with Tags");
        
        savedArticle.Should().NotBeNull();
        savedArticle!.Tags.Should().HaveCount(2);
        savedArticle.Tags.Should().Contain(t => t.Name == "C#");
        savedArticle.Tags.Should().Contain(t => t.Name == "ASP.NET");
    }

    [Fact]
    public async Task Article_ShouldSupportCommentsRelationship()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var article = new Article
        {
            Title = "Article with Comments",
            Content = "Content",
            Summary = "Summary",
            CreatedDate = DateTime.UtcNow,
            Status = ArticleStatus.Published
        };

        context.Articles.Add(article);
        await context.SaveChangesAsync();

        var comment = new Comment
        {
            AuthorName = "John Doe",
            AuthorEmail = "john@example.com",
            Content = "Great article!",
            SubmittedDate = DateTime.UtcNow,
            Status = CommentStatus.Pending,
            ArticleId = article.Id
        };

        // Act
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        // Assert
        var savedArticle = await context.Articles
            .Include(a => a.Comments)
            .FirstOrDefaultAsync(a => a.Title == "Article with Comments");
        
        savedArticle.Should().NotBeNull();
        savedArticle!.Comments.Should().HaveCount(1);
        savedArticle.Comments.First().AuthorName.Should().Be("John Doe");
        savedArticle.Comments.First().Status.Should().Be(CommentStatus.Pending);
    }
}