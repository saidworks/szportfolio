using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace PortfolioCMS.API.Services;

/// <summary>
/// Service for interacting with Azure Key Vault using Managed Identity
/// </summary>
public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultService> _logger;

    public KeyVaultService(IConfiguration configuration, ILogger<KeyVaultService> logger)
    {
        _logger = logger;

        var keyVaultUri = configuration["KeyVault:VaultUri"];
        if (string.IsNullOrWhiteSpace(keyVaultUri))
        {
            throw new InvalidOperationException("KeyVault:VaultUri is not configured");
        }

        // Use DefaultAzureCredential which supports:
        // - Managed Identity (in Azure)
        // - Azure CLI (local development)
        // - Visual Studio (local development)
        // - Environment variables
        var credential = new DefaultAzureCredential();
        _secretClient = new SecretClient(new Uri(keyVaultUri), credential);

        _logger.LogInformation("KeyVaultService initialized with URI: {VaultUri}", keyVaultUri);
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        try
        {
            _logger.LogDebug("Retrieving secret: {SecretName}", secretName);
            
            var secret = await _secretClient.GetSecretAsync(secretName);
            
            _logger.LogInformation("Successfully retrieved secret: {SecretName}", secretName);
            return secret.Value.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Secret not found: {SecretName}", secretName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        try
        {
            _logger.LogDebug("Setting secret: {SecretName}", secretName);
            
            await _secretClient.SetSecretAsync(secretName, secretValue);
            
            _logger.LogInformation("Successfully set secret: {SecretName}", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task DeleteSecretAsync(string secretName)
    {
        try
        {
            _logger.LogDebug("Deleting secret: {SecretName}", secretName);
            
            var operation = await _secretClient.StartDeleteSecretAsync(secretName);
            await operation.WaitForCompletionAsync();
            
            _logger.LogInformation("Successfully deleted secret: {SecretName}", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task<bool> SecretExistsAsync(string secretName)
    {
        try
        {
            await _secretClient.GetSecretAsync(secretName);
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if secret exists: {SecretName}", secretName);
            throw;
        }
    }
}
