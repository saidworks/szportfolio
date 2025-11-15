using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PortfolioCMS.API.DTOs;
using Xunit;

namespace PortfolioCMS.Tests.Security;

/// <summary>
/// Security tests for input validation and sanitization
/// </summary>
public class InputValidationSecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public InputValidationSecurityTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    [InlineData("<iframe src='javascript:alert(1)'></iframe>")]
    public async Task CreateComment_WithXssAttempt_ReturnsBadRequest(string maliciousContent)
    {
        // Arrange
        var commentDto = new CreateCommentDto
        {
            ArticleId = 1,
            AuthorName = "Test User",
            AuthorEmail = "test@example.com",
            Content = maliciousContent
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/comments", commentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("' OR '1'='1")]
    [InlineData("1; DROP TABLE Articles--")]
    [InlineData("1' UNION SELECT * FROM AspNetUsers--")]
    [InlineData("admin'--")]
    public async Task SearchArticles_WithSqlInjectionAttempt_ReturnsSafeResults(string maliciousQuery)
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/articles/search?query={Uri.EscapeDataString(maliciousQuery)}");

        // Assert
        // Should either return bad request or safe empty results, not execute SQL
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32\\config\\sam")]
    [InlineData("%2e%2e%2f%2e%2e%2f")]
    public async Task GetArticle_WithPathTraversalAttempt_ReturnsBadRequest(string maliciousPath)
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/articles/{maliciousPath}");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateComment_WithExcessivelyLongContent_ReturnsBadRequest()
    {
        // Arrange
        var commentDto = new CreateCommentDto
        {
            ArticleId = 1,
            AuthorName = "Test User",
            AuthorEmail = "test@example.com",
            Content = new string('a', 10000) // 10,000 characters
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/comments", commentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test..user@example.com")]
    public async Task CreateComment_WithInvalidEmail_ReturnsBadRequest(string invalidEmail)
    {
        // Arrange
        var commentDto = new CreateCommentDto
        {
            ArticleId = 1,
            AuthorName = "Test User",
            AuthorEmail = invalidEmail,
            Content = "Test comment"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/comments", commentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateComment_WithNullValues_ReturnsBadRequest()
    {
        // Arrange
        var commentDto = new CreateCommentDto
        {
            ArticleId = 1,
            AuthorName = null!,
            AuthorEmail = null!,
            Content = null!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/comments", commentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateComment_WithEmptyValues_ReturnsBadRequest()
    {
        // Arrange
        var commentDto = new CreateCommentDto
        {
            ArticleId = 1,
            AuthorName = "",
            AuthorEmail = "",
            Content = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/comments", commentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("<script>")]
    [InlineData("javascript:")]
    [InlineData("onerror=")]
    [InlineData("onload=")]
    public async Task CreateComment_WithDangerousPatterns_ReturnsBadRequest(string dangerousPattern)
    {
        // Arrange
        var commentDto = new CreateCommentDto
        {
            ArticleId = 1,
            AuthorName = "Test User",
            AuthorEmail = "test@example.com",
            Content = $"This is a test {dangerousPattern} comment"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/comments", commentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetArticle_WithNegativeId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/articles/-1");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetArticle_WithZeroId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/articles/0");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("1e10")]
    [InlineData("null")]
    public async Task GetArticle_WithInvalidIdFormat_ReturnsBadRequest(string invalidId)
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/articles/{invalidId}");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Request_WithSuspiciousUserAgent_IsBlocked()
    {
        // Arrange
        _client.DefaultRequestHeaders.UserAgent.Clear();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("sqlmap/1.0");

        // Act
        var response = await _client.GetAsync("/api/v1/articles");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("nikto")]
    [InlineData("nmap")]
    [InlineData("masscan")]
    public async Task Request_WithScannerUserAgent_IsBlocked(string scannerName)
    {
        // Arrange
        _client.DefaultRequestHeaders.UserAgent.Clear();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd(scannerName);

        // Act
        var response = await _client.GetAsync("/api/v1/articles");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }
}
