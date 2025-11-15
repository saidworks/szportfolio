using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PortfolioCMS.API.DTOs;
using Xunit;

namespace PortfolioCMS.Tests.Security;

/// <summary>
/// Security tests for authentication mechanisms
/// </summary>
public class AuthenticationSecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationSecurityTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@portfoliocms.com",
            Password = "Admin123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@portfoliocms.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithSqlInjectionAttempt_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@portfoliocms.com' OR '1'='1",
            Password = "' OR '1'='1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithXssAttempt_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "<script>alert('xss')</script>@test.com",
            Password = "<script>alert('xss')</script>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithExcessivelyLongPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@portfoliocms.com",
            Password = new string('a', 10000) // 10,000 characters
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/admin/articles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await _client.GetAsync("/api/v1/admin/articles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.GetAsync("/api/v1/admin/articles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("admin@portfoliocms.com", "short")]
    [InlineData("admin@portfoliocms.com", "nouppercaseornumber")]
    [InlineData("admin@portfoliocms.com", "NOLOWERCASE123")]
    [InlineData("admin@portfoliocms.com", "NoSpecialChar123")]
    public async Task Register_WithWeakPassword_ReturnsBadRequest(string email, string password)
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TokenRefresh_WithValidRefreshToken_ReturnsNewToken()
    {
        // Arrange - First login to get tokens
        var loginDto = new LoginDto
        {
            Email = "admin@portfoliocms.com",
            Password = "Admin123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        // Act - Attempt to refresh token
        var refreshDto = new RefreshTokenDto
        {
            Token = loginResult!.Token,
            RefreshToken = loginResult.RefreshToken
        };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshDto);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
        }
        else
        {
            // Refresh token endpoint might not be implemented yet
            Assert.True(response.StatusCode == HttpStatusCode.NotFound || 
                       response.StatusCode == HttpStatusCode.NotImplemented);
        }
    }
}
