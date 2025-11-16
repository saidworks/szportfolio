namespace PortfolioCMS.API.Services;

/// <summary>
/// Service interface for Microsoft Entra ID (formerly Azure Active Directory) operations
/// </summary>
public interface IAzureAdService
{
    /// <summary>
    /// Validates a Microsoft Entra ID access token
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Gets user information from Microsoft Entra ID token
    /// </summary>
    Task<EntraIdUserInfo?> GetUserInfoFromTokenAsync(string token);

    /// <summary>
    /// Checks if Microsoft Entra ID is configured and enabled
    /// </summary>
    bool IsEntraIdEnabled();
}

/// <summary>
/// Microsoft Entra ID user information
/// </summary>
public class EntraIdUserInfo
{
    public string ObjectId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Legacy alias for backward compatibility
/// </summary>
[Obsolete("Use EntraIdUserInfo instead. Azure AD has been renamed to Microsoft Entra ID.")]
public class AzureAdUserInfo : EntraIdUserInfo
{
}
