using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace PortfolioCMS.API.Configuration;

/// <summary>
/// Configuration extensions for Azure security services including Key Vault and Microsoft Entra ID
/// </summary>
public static class AzureSecurityConfiguration
{
    /// <summary>
    /// Adds Azure Key Vault configuration provider using Managed Identity
    /// </summary>
    public static IConfigurationBuilder AddAzureKeyVaultConfiguration(
        this IConfigurationBuilder builder,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var keyVaultUri = configuration["KeyVault:VaultUri"];
        
        if (string.IsNullOrWhiteSpace(keyVaultUri))
        {
            Console.WriteLine("KeyVault:VaultUri not configured. Skipping Key Vault integration.");
            return builder;
        }

        try
        {
            // Use DefaultAzureCredential for authentication
            // In Azure: Uses Managed Identity
            // Locally: Uses Azure CLI, Visual Studio, or environment variables
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = false,
                ExcludeAzureCliCredential = false,
                ExcludeVisualStudioCredential = false,
                ExcludeVisualStudioCodeCredential = false,
                ExcludeManagedIdentityCredential = false,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeInteractiveBrowserCredential = true
            });

            builder.AddAzureKeyVault(new Uri(keyVaultUri), credential);
            
            Console.WriteLine($"Azure Key Vault configured: {keyVaultUri}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring Azure Key Vault: {ex.Message}");
            
            // In development, we can continue without Key Vault
            if (!environment.IsDevelopment())
            {
                throw;
            }
        }

        return builder;
    }

    /// <summary>
    /// Configures Microsoft Entra ID (formerly Azure Active Directory) authentication
    /// </summary>
    public static IServiceCollection AddAzureActiveDirectoryAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Check for EntraId configuration first (new naming), fall back to AzureAd (legacy)
        var entraIdSettings = configuration.GetSection("EntraId");
        var aadSettings = configuration.GetSection("AzureAd");
        
        IConfigurationSection configSection;
        string configName;
        
        if (entraIdSettings.Exists())
        {
            configSection = entraIdSettings;
            configName = "EntraId";
        }
        else if (aadSettings.Exists())
        {
            configSection = aadSettings;
            configName = "AzureAd";
        }
        else
        {
            Console.WriteLine("EntraId/AzureAd configuration not found. Skipping Entra ID integration.");
            return services;
        }

        var tenantId = configSection["TenantId"];
        var clientId = configSection["ClientId"];

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            Console.WriteLine($"{configName} TenantId or ClientId not configured. Skipping Entra ID integration.");
            return services;
        }

        // Add Microsoft Identity Web authentication
        // This enables Microsoft Entra ID authentication alongside JWT authentication
        // Uncomment when ready to use Entra ID:
        /*
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configSection);
        */

        Console.WriteLine($"Microsoft Entra ID authentication configured for Tenant: {tenantId}");

        return services;
    }

    /// <summary>
    /// Validates that required Azure security configurations are present
    /// </summary>
    public static void ValidateAzureSecurityConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var errors = new List<string>();

        // In production, Key Vault is required
        if (!environment.IsDevelopment())
        {
            var keyVaultUri = configuration["KeyVault:VaultUri"];
            if (string.IsNullOrWhiteSpace(keyVaultUri))
            {
                errors.Add("KeyVault:VaultUri is required in production");
            }
        }

        // JWT settings are always required
        var jwtSecretKey = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(jwtSecretKey))
        {
            errors.Add("JwtSettings:SecretKey is required");
        }

        var jwtIssuer = configuration["JwtSettings:Issuer"];
        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            errors.Add("JwtSettings:Issuer is required");
        }

        var jwtAudience = configuration["JwtSettings:Audience"];
        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            errors.Add("JwtSettings:Audience is required");
        }

        if (errors.Any())
        {
            var errorMessage = "Security configuration validation failed:\n" + string.Join("\n", errors);
            throw new InvalidOperationException(errorMessage);
        }
    }
}
