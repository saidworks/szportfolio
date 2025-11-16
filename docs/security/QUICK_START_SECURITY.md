# Quick Start: Azure Security Services

This guide provides a quick reference for developers working with the Portfolio CMS security features.

## Local Development Setup

### Prerequisites
- Azure CLI installed and logged in
- Access to Azure subscription
- Visual Studio or VS Code with Azure extensions

### Step 1: Configure Local Secrets

For local development, use User Secrets instead of Key Vault:

```bash
# Navigate to API project
cd src/PortfolioCMS.API

# Initialize user secrets
dotnet user-secrets init

# Set required secrets
dotnet user-secrets set "JwtSettings:SecretKey" "YourLocalDevelopmentSecretKey32Chars!"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=PortfolioCMS_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
```

### Step 2: Run Locally

```bash
# The application will use local secrets and skip Key Vault
dotnet run
```

## Azure Deployment Quick Start

### Step 1: Deploy Infrastructure

```bash
cd infrastructure

# Initialize Terraform
terraform init

# Create terraform.tfvars
cat > terraform.tfvars <<EOF
environment = "development"
resource_group_name = "rg-portfoliocms-dev"
key_vault_name = "kv-portfoliocms-dev"
sql_server_name = "sql-portfoliocms-dev"
sql_admin_password = "CHANGE_ME_SECURE_PASSWORD_123!"
alert_email_address = "your-email@example.com"
EOF

# Deploy
terraform plan -out=tfplan
terraform apply tfplan
```

### Step 2: Initialize Secrets

```powershell
# Run the automated setup script
.\scripts\setup-azure-security.ps1 `
  -ResourceGroupName "rg-portfoliocms-dev" `
  -KeyVaultName "kv-portfoliocms-dev" `
  -SqlServerName "sql-portfoliocms-dev" `
  -SqlDatabaseName "sqldb-portfoliocms" `
  -FrontendAppName "app-portfoliocms-frontend" `
  -ApiAppName "app-portfoliocms-api" `
  -StorageAccountName "stportfoliocmsmedia" `
  -Environment "development"
```

### Step 3: Update Application Settings

Update `appsettings.json` with your Key Vault URI:

```json
{
  "KeyVault": {
    "VaultUri": "https://kv-portfoliocms-dev.vault.azure.net/"
  }
}
```

### Step 4: Deploy Application

```bash
# Deploy API
az webapp deployment source config-zip \
  --resource-group rg-portfoliocms-dev \
  --name app-portfoliocms-api \
  --src api.zip

# Deploy Frontend
az webapp deployment source config-zip \
  --resource-group rg-portfoliocms-dev \
  --name app-portfoliocms-frontend \
  --src frontend.zip
```

## Common Tasks

### Retrieve a Secret from Key Vault

```bash
az keyvault secret show \
  --vault-name kv-portfoliocms-dev \
  --name SqlAdminPassword \
  --query value \
  --output tsv
```

### Update a Secret

```bash
az keyvault secret set \
  --vault-name kv-portfoliocms-dev \
  --name JwtSecretKey \
  --value "NewSecretValue"
```

### Check Managed Identity

```bash
# Get managed identity principal ID
az webapp identity show \
  --resource-group rg-portfoliocms-dev \
  --name app-portfoliocms-api \
  --query principalId \
  --output tsv
```

### Grant Key Vault Access

```bash
# Grant access to a managed identity
az keyvault set-policy \
  --name kv-portfoliocms-dev \
  --object-id <PRINCIPAL_ID> \
  --secret-permissions get list
```

## Using Key Vault in Code

### Retrieve a Secret

```csharp
// Inject IKeyVaultService
public class MyService
{
    private readonly IKeyVaultService _keyVaultService;
    
    public MyService(IKeyVaultService keyVaultService)
    {
        _keyVaultService = keyVaultService;
    }
    
    public async Task<string> GetConnectionStringAsync()
    {
        return await _keyVaultService.GetSecretAsync("SqlConnectionString");
    }
}
```

### Configuration Automatically Loads from Key Vault

```csharp
// Secrets are automatically loaded into IConfiguration
public class MyService
{
    private readonly IConfiguration _configuration;
    
    public MyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GetJwtSecret()
    {
        // This automatically retrieves from Key Vault if configured
        return _configuration["JwtSettings:SecretKey"];
    }
}
```

## Microsoft Entra ID (Optional)

### Enable Entra ID Authentication

1. Register apps in Entra ID (see AZURE_SECURITY_SETUP.md)
2. Store credentials in Key Vault
3. Uncomment in `Program.cs`:

```csharp
// Uncomment this line:
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(configuration.GetSection("EntraId"));
```

4. Update `appsettings.json`:

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CallbackPath": "/signin-oidc"
  }
}
```

## WAF Configuration (Production)

### Enable Application Gateway

Update `terraform.tfvars`:

```hcl
enable_application_gateway = true
waf_mode = "Detection"  # Use "Prevention" for production
```

### Monitor WAF Logs

```bash
az monitor log-analytics query \
  --workspace law-portfoliocms \
  --analytics-query "AzureDiagnostics | where Category == 'ApplicationGatewayFirewallLog' | take 100"
```

## Troubleshooting

### "Cannot access Key Vault"

**Check:**
1. Key Vault URI is correct in appsettings.json
2. Managed identity is enabled on App Service
3. Access policy is granted to managed identity
4. You're logged in with Azure CLI for local development

**Fix:**
```bash
# Login to Azure CLI
az login

# Verify access
az keyvault secret list --vault-name kv-portfoliocms-dev
```

### "Secret not found"

**Check:**
1. Secret exists in Key Vault
2. Secret name matches exactly (case-sensitive)

**Fix:**
```bash
# List all secrets
az keyvault secret list --vault-name kv-portfoliocms-dev --output table

# Create missing secret
az keyvault secret set \
  --vault-name kv-portfoliocms-dev \
  --name MissingSecretName \
  --value "SecretValue"
```

### "Managed Identity authentication failed"

**Check:**
1. App Service has system-assigned managed identity enabled
2. Identity has Key Vault access policy

**Fix:**
```bash
# Enable managed identity
az webapp identity assign \
  --resource-group rg-portfoliocms-dev \
  --name app-portfoliocms-api

# Get principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --resource-group rg-portfoliocms-dev \
  --name app-portfoliocms-api \
  --query principalId \
  --output tsv)

# Grant access
az keyvault set-policy \
  --name kv-portfoliocms-dev \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

## Security Checklist

Before deploying to production:

- [ ] All secrets stored in Key Vault (no hardcoded values)
- [ ] Managed identities enabled for all App Services
- [ ] Key Vault access policies configured
- [ ] WAF enabled in Prevention mode
- [ ] SSL certificates configured
- [ ] Security headers middleware enabled
- [ ] Rate limiting configured
- [ ] Input validation active
- [ ] Monitoring and alerting set up
- [ ] Backup and disaster recovery configured

## Additional Resources

- **Detailed Setup**: `docs/security/AZURE_SECURITY_SETUP.md`
- **Architecture**: `docs/security/SECURITY_CONFIGURATION.md`
- **Task Summary**: `docs/security/TASK_6.2_COMPLETION_SUMMARY.md`

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review detailed documentation in `docs/security/`
3. Check Azure Portal for resource status
4. Review Application Insights logs for errors
