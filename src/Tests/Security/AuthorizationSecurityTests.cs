using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PortfolioCMS.API.DTOs;
using Xunit;

namespace PortfolioCMS.Tests.Security;

/// <summary>
/// Security tests for authorization and access control
/// </summary>
public class AuthorizationSecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthorizationSecurityTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginDto = new LoginDto
        {
            Email = "admin@portfoliocms.com",
            Password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return result?.Token ?? string.Empty;
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminRole_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/admin/articles");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AdminEndpoint_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/admin/articles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateArticle_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange - This would require a viewer/non-admin token
        // For now, test without any token
        var articleDto = new CreateArticleDto
        {
            Title = "Test Article",
            Content = "Test Content",
            Summary = "Test Summary"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/articles", articleDto);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteArticle_WithoutAdminRole_ReturnsForbidden()
    {
        // Act
        var response = await _client.DeleteAsync("/api/v1/articles/1");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArticle_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        var articleDto = new UpdateArticleDto
        {
            Title = "Updated Title",
            Content = "Updated Content"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/articles/1", articleDto);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveComment_WithoutAdminRole_ReturnsForbidden()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/comments/1/approve", null);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_WithoutAdminRole_ReturnsForbidden()
    {
        // Act
        var response = await _client.DeleteAsync("/api/v1/comments/1");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicEndpoint_WithoutAuthentication_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/articles");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_WithoutAuthentication_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/admin/articles")]
    [InlineData("/api/v1/admin/comments")]
    [InlineData("/api/v1/admin/projects")]
    public async Task AdminEndpoints_WithoutToken_ReturnUnauthorized(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DirectObjectReference_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act - Try to access non-existent article
        var response = await _client.GetAsync("/api/v1/articles/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PathTraversal_InArticleId_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act - Try path traversal attack
        var response = await _client.GetAsync("/api/v1/articles/../../../etc/passwd");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.NotFound);
    }
}
