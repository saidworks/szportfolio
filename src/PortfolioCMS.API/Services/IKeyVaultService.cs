namespace PortfolioCMS.API.Services;

/// <summary>
/// Service interface for Azure Key Vault operations
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Retrieves a secret from Azure Key Vault
    /// </summary>
    Task<string?> GetSecretAsync(string secretName);

    /// <summary>
    /// Sets a secret in Azure Key Vault
    /// </summary>
    Task SetSecretAsync(string secretName, string secretValue);

    /// <summary>
    /// Deletes a secret from Azure Key Vault
    /// </summary>
    Task DeleteSecretAsync(string secretName);

    /// <summary>
    /// Checks if a secret exists in Azure Key Vault
    /// </summary>
    Task<bool> SecretExistsAsync(string secretName);
}
