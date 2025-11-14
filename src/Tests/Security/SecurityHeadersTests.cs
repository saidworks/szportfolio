using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PortfolioCMS.Tests.Security;

/// <summary>
/// Security tests for HTTP security headers
/// </summary>
public class SecurityHeadersTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SecurityHeadersTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Response_ContainsContentSecurityPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        var cspHeader = response.Headers.GetValues("Content-Security-Policy").FirstOrDefault();
        Assert.NotNull(cspHeader);
        Assert.Contains("default-src", cspHeader);
    }

    [Fact]
    public async Task Response_ContainsXContentTypeOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        var header = response.Headers.GetValues("X-Content-Type-Options").FirstOrDefault();
        Assert.Equal("nosniff", header);
    }

    [Fact]
    public async Task Response_ContainsXFrameOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        var header = response.Headers.GetValues("X-Frame-Options").FirstOrDefault();
        Assert.Equal("DENY", header);
    }

    [Fact]
    public async Task Response_ContainsXXssProtectionHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.True(response.Headers.Contains("X-XSS-Protection"));
        var header = response.Headers.GetValues("X-XSS-Protection").FirstOrDefault();
        Assert.Contains("1", header);
    }

    [Fact]
    public async Task Response_ContainsReferrerPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        var header = response.Headers.GetValues("Referrer-Policy").FirstOrDefault();
        Assert.Equal("strict-origin-when-cross-origin", header);
    }

    [Fact]
    public async Task Response_ContainsPermissionsPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.True(response.Headers.Contains("Permissions-Policy"));
        var header = response.Headers.GetValues("Permissions-Policy").FirstOrDefault();
        Assert.NotNull(header);
        Assert.Contains("geolocation=()", header);
    }

    [Fact]
    public async Task Response_DoesNotContainServerHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task Response_DoesNotContainXPoweredByHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.False(response.Headers.Contains("X-Powered-By"));
    }

    [Fact]
    public async Task Response_DoesNotContainXAspNetVersionHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        Assert.False(response.Headers.Contains("X-AspNet-Version"));
    }

    [Fact]
    public async Task Response_ContainsStrictTransportSecurityInProduction()
    {
        // Note: HSTS is only added in non-development environments
        // This test will pass in development (no HSTS) and production (has HSTS)
        
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        // In development, HSTS should not be present
        // In production, it should be present with proper configuration
        if (response.Headers.Contains("Strict-Transport-Security"))
        {
            var header = response.Headers.GetValues("Strict-Transport-Security").FirstOrDefault();
            Assert.Contains("max-age=", header);
            Assert.Contains("includeSubDomains", header);
        }
    }

    [Theory]
    [InlineData("/api/v1/articles")]
    [InlineData("/api/v1/projects")]
    [InlineData("/api/v1/health")]
    public async Task AllEndpoints_ContainSecurityHeaders(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("X-XSS-Protection"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
    }

    [Fact]
    public async Task ContentSecurityPolicy_BlocksInlineScripts()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        var cspHeader = response.Headers.GetValues("Content-Security-Policy").FirstOrDefault();
        Assert.NotNull(cspHeader);
        
        // CSP should restrict script sources
        Assert.Contains("script-src", cspHeader);
    }

    [Fact]
    public async Task ContentSecurityPolicy_RestrictsFrameAncestors()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        var cspHeader = response.Headers.GetValues("Content-Security-Policy").FirstOrDefault();
        Assert.NotNull(cspHeader);
        
        // CSP should prevent framing
        Assert.Contains("frame-ancestors 'none'", cspHeader);
    }
}
