using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Configuration helper for Azure Key Vault integration with Managed Identity.
/// Provides secure access to database credentials and connection strings.
/// </summary>
public static class AzureKeyVaultConfiguration
{
    /// <summary>
    /// Adds Azure Key Vault configuration provider using Managed Identity.
    /// This allows the application to retrieve secrets (like database passwords) securely.
    /// </summary>
    /// <param name="configuration">Configuration builder</param>
    /// <param name="keyVaultUri">Azure Key Vault URI (e.g., https://myvault.vault.azure.net/)</param>
    /// <returns>Configuration builder for chaining</returns>
    public static IConfigurationBuilder AddAzureKeyVaultWithManagedIdentity(
        this IConfigurationBuilder configuration,
        string keyVaultUri)
    {
        if (string.IsNullOrEmpty(keyVaultUri))
        {
            throw new ArgumentException("Key Vault URI cannot be null or empty", nameof(keyVaultUri));
        }

        // Use DefaultAzureCredential which supports:
        // 1. Managed Identity (in Azure)
        // 2. Azure CLI (local development)
        // 3. Visual Studio (local development)
        // 4. Environment variables (CI/CD)
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            // Exclude interactive browser credential for non-interactive scenarios
            ExcludeInteractiveBrowserCredential = true,
            // Retry options for transient failures
            Retry =
            {
                MaxRetries = 3,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(10),
                Mode = Azure.Core.RetryMode.Exponential
            }
        });

        configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);

        return configuration;
    }

    /// <summary>
    /// Validates that Key Vault configuration is properly set up.
    /// </summary>
    /// <param name="configuration">Configuration to validate</param>
    /// <returns>True if Key Vault is configured, false otherwise</returns>
    public static bool IsKeyVaultConfigured(IConfiguration configuration)
    {
        var keyVaultUri = configuration["KeyVault:VaultUri"] 
                         ?? configuration["Azure:KeyVault:VaultUri"];
        
        return !string.IsNullOrEmpty(keyVaultUri);
    }

    /// <summary>
    /// Gets the Key Vault URI from configuration.
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <returns>Key Vault URI or null if not configured</returns>
    public static string? GetKeyVaultUri(IConfiguration configuration)
    {
        return configuration["KeyVault:VaultUri"] 
               ?? configuration["Azure:KeyVault:VaultUri"];
    }
}
