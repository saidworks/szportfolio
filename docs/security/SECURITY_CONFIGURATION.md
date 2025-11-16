# Security Configuration Guide

This document provides comprehensive information about the security configuration for the Portfolio CMS application.

## Overview

The Portfolio CMS implements a multi-layered security approach:

1. **Azure Key Vault** - Centralized secrets management
2. **Managed Identity** - Passwordless authentication to Azure services
3. **Microsoft Entra ID** - Enterprise identity and access management (optional)
4. **Application Gateway WAF** - Web application firewall protection
5. **Application-level Security** - Input validation, rate limiting, security headers

## Security Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Internet Traffic                         │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              Azure Application Gateway (WAF)                 │
│  • OWASP Core Rule Set 3.2                                  │
│  • Custom Rate Limiting Rules                               │
│  • Geo-Filtering                                            │
│  • Bot Protection                                           │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    App Services Layer                        │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │  Frontend App    │         │    API App       │         │
│  │  (Blazor Server) │◄───────►│  (ASP.NET Core)  │         │
│  │                  │         │                  │         │
│  │  Managed Identity│         │  Managed Identity│         │
│  └────────┬─────────┘         └────────┬─────────┘         │
└───────────┼──────────────────────────────┼──────────────────┘
            │                              │
            │         ┌────────────────────┘
            │         │
            ▼         ▼
┌─────────────────────────────────────────────────────────────┐
│                   Azure Key Vault                            │
│  • SQL Connection Strings                                   │
│  • JWT Secret Keys                                          │
│  • Storage Account Keys                                     │
│  • SSL Certificates                                         │
│  • Azure AD Credentials                                     │
└─────────────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Protected Resources                        │
│  • Azure SQL Database                                       │
│  • Azure Blob Storage                                       │
│  • Application Insights                                     │
└─────────────────────────────────────────────────────────────┘
```

## 1. Azure Key Vault Configuration

### Purpose
Azure Key Vault provides centralized, secure storage for:
- Database passwords and connection strings
- JWT secret keys
- Storage account connection strings
- SSL certificates
- API keys and other sensitive configuration

### Configuration

**Location**: `infrastructure/main.tf`

```hcl
resource "azurerm_key_vault" "main" {
  name                        = var.key_vault_name
  location                    = azurerm_resource_group.main.location
  resource_group_name         = azurerm_resource_group.main.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  sku_name                    = "standard"
}
```

### Secrets Stored

| Secret Name | Purpose | Rotation Frequency |
|------------|---------|-------------------|
| `SqlAdminPassword` | SQL Server admin password | Every 90 days |
| `SqlConnectionString` | Complete SQL connection string | When password rotates |
| `JwtSecretKey` | JWT token signing key | Every 180 days |
| `StorageConnectionString` | Azure Blob Storage access | Every 90 days |
| `SslCertificateData` | SSL certificate for HTTPS | Before expiration |
| `SslCertificatePassword` | SSL certificate password | With certificate |
| `EntraId--TenantId` | Microsoft Entra ID tenant identifier | Never (unless migrating) |
| `EntraId--ClientId` | Microsoft Entra ID application ID | Never (unless recreating) |
| `AzureAd--TenantId` | Legacy: Entra ID tenant identifier | Never (unless migrating) |
| `AzureAd--ClientId` | Legacy: Entra ID application ID | Never (unless recreating) |

### Access Policies

Access to Key Vault is granted via managed identities:

```hcl
resource "azurerm_key_vault_access_policy" "api" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.api.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}
```

### Application Integration

**Location**: `src/PortfolioCMS.API/Configuration/AzureSecurityConfiguration.cs`

```csharp
public static IConfigurationBuilder AddAzureKeyVaultConfiguration(
    this IConfigurationBuilder builder,
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    var keyVaultUri = configuration["KeyVault:VaultUri"];
    
    if (!string.IsNullOrWhiteSpace(keyVaultUri))
    {
        var credential = new DefaultAzureCredential();
        builder.AddAzureKeyVault(new Uri(keyVaultUri), credential);
    }
    
    return builder;
}
```

## 2. Managed Identity Configuration

### Purpose
Managed identities eliminate the need to store credentials in code or configuration files. They provide automatic credential management for Azure services.

### Types Used

**System-Assigned Managed Identity**
- Automatically created with App Service
- Lifecycle tied to the App Service
- Used for Key Vault access

### Configuration

Managed identities are automatically enabled in Terraform:

```hcl
resource "azurerm_linux_web_app" "api" {
  name                = var.api_app_name
  # ... other configuration ...
  
  identity {
    type = "SystemAssigned"
  }
}
```

### Usage in Application

**Location**: `src/PortfolioCMS.API/Services/KeyVaultService.cs`

```csharp
public KeyVaultService(IConfiguration configuration, ILogger<KeyVaultService> logger)
{
    var keyVaultUri = configuration["KeyVault:VaultUri"];
    
    // DefaultAzureCredential automatically uses Managed Identity in Azure
    // and falls back to Azure CLI/Visual Studio for local development
    var credential = new DefaultAzureCredential();
    _secretClient = new SecretClient(new Uri(keyVaultUri), credential);
}
```

### Benefits

1. **No credential storage** - No passwords or keys in code
2. **Automatic rotation** - Azure manages credential lifecycle
3. **Audit trail** - All access logged in Azure Monitor
4. **Least privilege** - Granular permissions per identity

## 3. Microsoft Entra ID Integration

### Purpose
Microsoft Entra ID (formerly Azure Active Directory) provides enterprise-grade authentication and authorization (optional feature).

### Current Status
The application currently uses JWT tokens with ASP.NET Core Identity. Microsoft Entra ID integration is prepared but not activated.

### Configuration

**Location**: `appsettings.json`

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "[Retrieved from Key Vault]",
    "ClientId": "[Retrieved from Key Vault]",
    "CallbackPath": "/signin-oidc",
    "Audience": "api://[ClientId]"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "[Retrieved from Key Vault]",
    "ClientId": "[Retrieved from Key Vault]",
    "CallbackPath": "/signin-oidc"
  }
}
```

**Note**: Both `EntraId` and `AzureAd` configuration sections are supported for backward compatibility.

### Enabling Microsoft Entra ID

To enable Microsoft Entra ID authentication:

1. Register applications in Microsoft Entra ID (see `AZURE_SECURITY_SETUP.md`)
2. Store credentials in Key Vault
3. Uncomment Entra ID configuration in `Program.cs`
4. Update authentication middleware

### Service Implementation

**Location**: `src/PortfolioCMS.API/Services/AzureAdService.cs`

The `AzureAdService` provides:
- Token validation
- User information extraction
- Role mapping from Microsoft Entra ID groups

## 4. Application Gateway WAF Configuration

### Purpose
Web Application Firewall (WAF) protects against common web vulnerabilities and attacks.

### Protection Layers

1. **OWASP Core Rule Set 3.2**
   - SQL Injection protection
   - Cross-Site Scripting (XSS) protection
   - Remote File Inclusion (RFI) protection
   - Local File Inclusion (LFI) protection
   - Command Injection protection

2. **Custom Rules**
   - Rate limiting (100 requests/minute per IP)
   - Suspicious user agent blocking
   - Known malicious IP blocking
   - Geographic filtering

3. **Bot Protection**
   - Microsoft Bot Manager Rule Set
   - Automated bot detection and blocking

### Configuration

**Location**: `infrastructure/modules/waf-policies/main.tf`

```hcl
resource "azurerm_web_application_firewall_policy" "main" {
  name                = var.waf_policy_name
  resource_group_name = var.resource_group_name
  location            = var.location

  policy_settings {
    enabled                     = true
    mode                        = var.waf_mode  # Detection or Prevention
    request_body_check          = true
    file_upload_limit_in_mb     = 100
    max_request_body_size_in_kb = 128
  }

  managed_rules {
    managed_rule_set {
      type    = "OWASP"
      version = "3.2"
    }
  }
}
```

### WAF Modes

**Detection Mode** (Development/Staging)
- Logs threats but doesn't block
- Used for testing and tuning
- Prevents false positives

**Prevention Mode** (Production)
- Actively blocks malicious requests
- Recommended for production
- Requires proper tuning

### Monitoring WAF

WAF logs are sent to Log Analytics:

```bash
az monitor log-analytics query \
  --workspace "law-portfoliocms" \
  --analytics-query "AzureDiagnostics | where Category == 'ApplicationGatewayFirewallLog'"
```

## 5. Application-Level Security

### Security Headers

**Location**: `src/PortfolioCMS.API/Middleware/SecurityHeadersMiddleware.cs`

Implemented headers:
- `Content-Security-Policy` - Prevents XSS attacks
- `X-Frame-Options` - Prevents clickjacking
- `X-Content-Type-Options` - Prevents MIME sniffing
- `Referrer-Policy` - Controls referrer information
- `Strict-Transport-Security` - Enforces HTTPS
- `X-XSS-Protection` - Browser XSS protection

### Input Validation

**Location**: `src/PortfolioCMS.API/Middleware/InputValidationMiddleware.cs`

- Validates all input against whitelist patterns
- Sanitizes HTML content
- Prevents SQL injection
- Prevents XSS attacks

### Rate Limiting

**Location**: `src/PortfolioCMS.API/Middleware/RateLimitingMiddleware.cs`

- 100 requests per minute per IP (configurable)
- Sliding window algorithm
- Returns 429 Too Many Requests when exceeded

### CORS Configuration

**Location**: `src/PortfolioCMS.API/Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? new[] { "https://localhost:7001" };
        
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();
    });
});
```

## Security Best Practices

### Development Environment

1. **Never commit secrets** to source control
2. **Use local secrets** for development (User Secrets, environment variables)
3. **Test with Detection mode** WAF before enabling Prevention
4. **Use Azure CLI credentials** for local Key Vault access

### Staging Environment

1. **Mirror production security** configuration
2. **Test secret rotation** procedures
3. **Validate WAF rules** don't block legitimate traffic
4. **Run security scans** (OWASP ZAP, SonarCloud)

### Production Environment

1. **Enable WAF Prevention mode**
2. **Rotate secrets regularly** (90-180 days)
3. **Monitor security logs** daily
4. **Enable Azure Security Center**
5. **Configure backup and disaster recovery**
6. **Implement least privilege access**
7. **Enable soft delete** on Key Vault
8. **Use production-grade SSL certificates**
9. **Configure geo-redundancy** for critical resources
10. **Document incident response** procedures

## Secret Rotation Procedures

### SQL Password Rotation

```bash
# 1. Generate new password
NEW_PASSWORD=$(openssl rand -base64 32)

# 2. Update SQL Server
az sql server update \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --admin-password $NEW_PASSWORD

# 3. Update Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --value "$NEW_PASSWORD"

# 4. Update connection string
# 5. Restart App Services
```

### JWT Secret Rotation

```bash
# 1. Generate new secret
NEW_JWT_SECRET=$(openssl rand -base64 32)

# 2. Update Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "JwtSecretKey" \
  --value "$NEW_JWT_SECRET"

# 3. Restart App Services
az webapp restart --resource-group $RESOURCE_GROUP --name $API_APP_NAME
```

## Compliance and Auditing

### Audit Logs

All security-related events are logged:
- Key Vault access (Azure Monitor)
- WAF blocks (Application Gateway logs)
- Authentication failures (Application Insights)
- API access (Application Insights)

### Compliance Standards

The security configuration supports:
- **OWASP Top 10** protection
- **PCI DSS** requirements (with additional configuration)
- **GDPR** data protection requirements
- **SOC 2** security controls

## Troubleshooting

See `AZURE_SECURITY_SETUP.md` for detailed troubleshooting steps.

## Additional Resources

- [Azure Key Vault Best Practices](https://docs.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [Managed Identity Best Practices](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/managed-identity-best-practice-recommendations)
- [Azure WAF Best Practices](https://docs.microsoft.com/en-us/azure/web-application-firewall/ag/best-practices)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
