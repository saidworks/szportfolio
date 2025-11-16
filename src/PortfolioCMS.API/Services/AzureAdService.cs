using Microsoft.Identity.Web;
using System.Security.Claims;

namespace PortfolioCMS.API.Services;

/// <summary>
/// Service for Microsoft Entra ID (formerly Azure Active Directory) authentication and authorization
/// </summary>
public class AzureAdService : IAzureAdService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureAdService> _logger;
    private readonly bool _isEnabled;

    public AzureAdService(IConfiguration configuration, ILogger<AzureAdService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Check if Entra ID is configured (check both new and legacy config names)
        var tenantId = _configuration["EntraId:TenantId"] ?? _configuration["AzureAd:TenantId"];
        var clientId = _configuration["EntraId:ClientId"] ?? _configuration["AzureAd:ClientId"];
        _isEnabled = !string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientId);

        if (_isEnabled)
        {
            _logger.LogInformation("Microsoft Entra ID authentication is enabled");
        }
        else
        {
            _logger.LogInformation("Microsoft Entra ID authentication is not configured. Using JWT authentication.");
        }
    }

    public bool IsEntraIdEnabled()
    {
        return _isEnabled;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("Microsoft Entra ID is not enabled. Cannot validate token.");
            return false;
        }

        try
        {
            // Token validation is handled by Microsoft.Identity.Web middleware
            // This method is for additional custom validation if needed
            _logger.LogDebug("Validating Microsoft Entra ID token");
            
            // Additional validation logic can be added here
            // For now, we rely on the middleware validation
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Microsoft Entra ID token");
            return false;
        }
    }

    public async Task<EntraIdUserInfo?> GetUserInfoFromTokenAsync(string token)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("Microsoft Entra ID is not enabled. Cannot get user info.");
            return null;
        }

        try
        {
            // In a real implementation, you would decode the JWT token
            // and extract user information from claims
            // For now, this is a placeholder that would be called from a controller
            // where ClaimsPrincipal is available
            
            _logger.LogDebug("Extracting user info from Microsoft Entra ID token");
            
            // This would typically be done in the controller with User.Claims
            // Example implementation:
            // var userInfo = new EntraIdUserInfo
            // {
            //     ObjectId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            //     Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            //     DisplayName = User.FindFirst("name")?.Value ?? string.Empty,
            //     GivenName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty,
            //     Surname = User.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty,
            //     Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            // };
            
            return await Task.FromResult<EntraIdUserInfo?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user info from Microsoft Entra ID token");
            return null;
        }
    }

    /// <summary>
    /// Helper method to extract user info from ClaimsPrincipal
    /// This should be called from controllers where User is available
    /// </summary>
    public static EntraIdUserInfo GetUserInfoFromClaims(ClaimsPrincipal user)
    {
        return new EntraIdUserInfo
        {
            ObjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                      user.FindFirst("oid")?.Value ?? 
                      string.Empty,
            Email = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                   user.FindFirst("preferred_username")?.Value ?? 
                   string.Empty,
            DisplayName = user.FindFirst("name")?.Value ?? 
                         user.FindFirst(ClaimTypes.Name)?.Value ?? 
                         string.Empty,
            GivenName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? 
                       user.FindFirst("given_name")?.Value ?? 
                       string.Empty,
            Surname = user.FindFirst(ClaimTypes.Surname)?.Value ?? 
                     user.FindFirst("family_name")?.Value ?? 
                     string.Empty,
            Roles = user.FindAll(ClaimTypes.Role)
                       .Select(c => c.Value)
                       .ToList()
        };
    }
}
