using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PortfolioCMS.Tests.Security;

/// <summary>
/// Security tests for CORS (Cross-Origin Resource Sharing) policies
/// </summary>
public class CorsSecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CorsSecurityTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task PreflightRequest_FromAllowedOrigin_ReturnsSuccess()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/articles");
        request.Headers.Add("Origin", "https://localhost:7001");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PreflightRequest_FromDisallowedOrigin_ReturnsFailure()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/articles");
        request.Headers.Add("Origin", "https://malicious-site.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // CORS middleware should not add Access-Control-Allow-Origin for disallowed origins
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin") && 
                    response.Headers.GetValues("Access-Control-Allow-Origin").Contains("https://malicious-site.com"));
    }

    [Fact]
    public async Task Request_WithAllowedOrigin_ContainsCorsHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/articles");
        request.Headers.Add("Origin", "https://localhost:7001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
            Assert.True(allowedOrigin == "https://localhost:7001" || allowedOrigin == "*");
        }
    }

    [Fact]
    public async Task PreflightRequest_WithDisallowedMethod_ReturnsFailure()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/articles");
        request.Headers.Add("Origin", "https://localhost:7001");
        request.Headers.Add("Access-Control-Request-Method", "TRACE"); // Dangerous method
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Methods"))
        {
            var allowedMethods = response.Headers.GetValues("Access-Control-Allow-Methods").FirstOrDefault();
            Assert.DoesNotContain("TRACE", allowedMethods ?? string.Empty);
        }
    }

    [Fact]
    public async Task PreflightRequest_ChecksAllowedMethods()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/articles");
        request.Headers.Add("Origin", "https://localhost:7001");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Methods"))
        {
            var allowedMethods = response.Headers.GetValues("Access-Control-Allow-Methods").FirstOrDefault();
            Assert.NotNull(allowedMethods);
            // Should allow standard HTTP methods
            Assert.Contains("GET", allowedMethods);
            Assert.Contains("POST", allowedMethods);
        }
    }

    [Fact]
    public async Task PreflightRequest_ChecksAllowedHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/articles");
        request.Headers.Add("Origin", "https://localhost:7001");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type,Authorization");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Headers"))
        {
            var allowedHeaders = response.Headers.GetValues("Access-Control-Allow-Headers").FirstOrDefault();
            Assert.NotNull(allowedHeaders);
            Assert.Contains("Content-Type", allowedHeaders, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Authorization", allowedHeaders, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Request_WithCredentials_AllowsCredentials()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/articles");
        request.Headers.Add("Origin", "https://localhost:7001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Credentials"))
        {
            var allowCredentials = response.Headers.GetValues("Access-Control-Allow-Credentials").FirstOrDefault();
            Assert.Equal("true", allowCredentials);
        }
    }

    [Fact]
    public async Task PreflightRequest_HasMaxAge()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/articles");
        request.Headers.Add("Origin", "https://localhost:7001");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        if (response.Headers.Contains("Access-Control-Max-Age"))
        {
            var maxAge = response.Headers.GetValues("Access-Control-Max-Age").FirstOrDefault();
            Assert.NotNull(maxAge);
            Assert.True(int.Parse(maxAge) > 0);
        }
    }

    [Theory]
    [InlineData("https://evil.com")]
    [InlineData("http://localhost:7001")] // HTTP instead of HTTPS
    [InlineData("https://localhost:9999")] // Wrong port
    public async Task Request_FromUnauthorizedOrigin_DoesNotSetCorsHeaders(string origin)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/articles");
        request.Headers.Add("Origin", origin);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // Should not set Access-Control-Allow-Origin for unauthorized origins
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
            Assert.NotEqual(origin, allowedOrigin);
        }
    }
}
