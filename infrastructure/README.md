# Portfolio CMS Infrastructure

This directory contains the Infrastructure as Code (IaC) configuration for the Portfolio CMS application using tofu/OpenTofu.

## Architecture Overview

The infrastructure deploys a three-tier architecture on Azure:

- **Frontend**: Blazor Server application hosted on Azure App Service
- **API**: ASP.NET Core Web API hosted on Azure App Service with system-assigned managed identity
- **Data**: Azure SQL Database Free tier (32 MB storage) with Entity Framework Core
- **Security**: Azure Key Vault for storing database passwords and connection strings, accessed via managed identity

## Prerequisites

1. **Azure CLI** - Install and configure Azure CLI
2. **tofu/OpenTofu** - Install tofu or OpenTofu
3. **Azure Subscription** - Active Azure subscription with appropriate permissions

## Secrets Management with .infra/ Directory

⚠️ **IMPORTANT**: The `.infra/` directory in the workspace root is gitignored and used for storing sensitive infrastructure files.

### What is .infra/?

The `.infra/` directory is a local-only folder (excluded from version control) where you should store:

- **Scripts with actual credentials** - Deployment scripts, Azure CLI scripts with real values
- **Infrastructure planning documents** - Architecture diagrams with real IPs, network topology, service principals
- **Terraform/OpenTofu variable files** - *.tfvars files with actual secrets and resource names
- **Deployment logs** - Terraform apply logs, Azure CLI command history, deployment timestamps
- **Exported secrets** - Key Vault secret backups, certificates, SSH keys, API tokens

### Directory Structure

```
.infra/
├── README.md                           # Comprehensive guide for .infra/ usage
├── scripts/
│   ├── deploy-production.ps1          # Real deployment script with credentials
│   ├── setup-keyvault-secrets.ps1     # Populate Key Vault with actual secrets
│   ├── rotate-database-password.ps1   # Database password rotation script
│   └── backup-keyvault-secrets.ps1    # Export Key Vault secrets for backup
├── planning/
│   ├── production-architecture.md     # Production architecture with real details
│   ├── network-topology.md            # Network configuration with actual IPs
│   ├── service-principals.md          # Service principal credentials
│   └── resource-inventory.md          # Complete resource list with IDs
├── terraform/
│   ├── production.tfvars              # Real Terraform variables for production
│   ├── staging.tfvars                 # Real Terraform variables for staging
│   ├── development.tfvars             # Real Terraform variables for development
│   └── backend-config.tfvars          # Backend storage account configuration
├── logs/
│   ├── deployment-YYYY-MM-DD.log      # Deployment execution logs
│   ├── azure-cli-history.txt          # Azure CLI command history
│   └── terraform-apply-YYYY-MM-DD.log # Terraform apply logs
└── secrets/
    ├── keyvault-backup-YYYY-MM-DD.json # Key Vault secret exports
    ├── certificates/                   # SSL/TLS certificates
    └── service-principals/             # Service principal credentials
```

### Workflow for Managing Secrets

1. **Create scripts with real values in `.infra/`** first
2. **Test thoroughly** in your local environment
3. **Create sanitized templates** in the main repository (e.g., `scripts/*.template.ps1`)
4. **Replace actual values** with placeholders:
   - `<SUBSCRIPTION_ID>`
   - `<RESOURCE_GROUP_NAME>`
   - `<SQL_ADMIN_PASSWORD>`
   - `<KEY_VAULT_NAME>`
   - `<STORAGE_ACCOUNT_KEY>`

### Example: Script Template Pattern

**Working version** (`.infra/scripts/deploy-production.ps1`):
```powershell
$SUBSCRIPTION_ID = "12345678-1234-1234-1234-123456789abc"
$SQL_PASSWORD = "MySecureP@ssw0rd123!"
$KEY_VAULT_NAME = "kv-portfoliocms-prod-eastus"

az account set --subscription $SUBSCRIPTION_ID
az keyvault secret set --vault-name $KEY_VAULT_NAME --name "SqlAdminPassword" --value $SQL_PASSWORD
```

**Template version** (`scripts/deploy-production.template.ps1`):
```powershell
# Template: Replace placeholders with actual values before use
$SUBSCRIPTION_ID = "<YOUR_SUBSCRIPTION_ID>"
$SQL_PASSWORD = "<GENERATE_SECURE_PASSWORD>"
$KEY_VAULT_NAME = "<YOUR_KEY_VAULT_NAME>"

az account set --subscription $SUBSCRIPTION_ID
az keyvault secret set --vault-name $KEY_VAULT_NAME --name "SqlAdminPassword" --value $SQL_PASSWORD
```

### Security Best Practices

✅ **DO:**
- Store all sensitive files in `.infra/`
- Use strong, randomly generated passwords
- Rotate credentials regularly
- Keep deployment logs for audit purposes
- Use Azure Key Vault for production secrets
- Retrieve secrets from Key Vault in CI/CD pipelines

❌ **DON'T:**
- Commit `.infra/` contents to version control
- Share `.infra/` files via email or chat
- Store production credentials in development scripts
- Use the same passwords across environments
- Leave credentials in clipboard or terminal history

### Verification

To verify the `.infra/` directory is properly gitignored:

```bash
# Should output: .infra/
git check-ignore .infra/

# Should show no files
git status .infra/
```

### Emergency: Secrets Committed to Git

If you accidentally commit secrets to git:

1. **Immediately rotate all exposed credentials**
2. **Remove from git history** using `git filter-branch` or BFG Repo-Cleaner
3. **Force push** to remote repository (coordinate with team)
4. **Notify security team** if production credentials were exposed
5. **Review access logs** for unauthorized access

For comprehensive documentation, see **[.infra/README.md](../.infra/README.md)** in the workspace root.

## Quick Start

⚠️ **IMPORTANT**: Before deploying infrastructure, you MUST complete the manual setup steps. See [MANUAL_SETUP_GUIDE.md](./MANUAL_SETUP_GUIDE.md) for detailed instructions.

### Quick Start Summary

1. **Complete Manual Setup** (Required First Time)
   - Follow [MANUAL_SETUP_GUIDE.md](./MANUAL_SETUP_GUIDE.md) to:
     - Create Key Vault and store database password
     - Set up Terraform state storage
     - Create service principal for CI/CD
     - Configure SSL certificates (optional)

2. **Deploy Infrastructure with Terraform**
   - See instructions below for environment-specific deployment

### 1. Configure Azure Authentication

```bash
# Login to Azure
az login

# Set the subscription (if you have multiple)
az account set --subscription "your-subscription-id"
```

### 2. Initialize tofu Backend

Before running tofu, you need to create the storage account for state management:

```bash
# Create resource group for tofu state
az group create --name rg-portfoliocms-tfstate --location "East US"

# Create storage account for tofu state
az storage account create \
  --resource-group rg-portfoliocms-tfstate \
  --name stportfoliocmstfstate \
  --sku Standard_LRS \
  --encryption-services blob

# Create storage container
az storage container create \
  --name tfstate \
  --account-name stportfoliocmstfstate
```

### 3. **CRITICAL: Initialize Key Vault Secrets Manually**

⚠️ **IMPORTANT**: Before deploying infrastructure with Terraform/OpenTofu, you must manually create the Key Vault and store the database password. This is a security best practice to avoid storing sensitive credentials in Terraform state or variables.

#### Step 3.1: Generate a Secure Database Password

```bash
# Generate a strong random password (Windows PowerShell)
$password = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 16 | ForEach-Object {[char]$_})
echo $password

# Or use a password manager to generate a secure password
# Requirements: At least 8 characters, mix of uppercase, lowercase, numbers, and special characters
```

#### Step 3.2: Create Key Vault and Store Database Password

```bash
# Set variables
$RESOURCE_GROUP="rg-portfoliocms-dev"
$LOCATION="eastus"
$KEY_VAULT_NAME="kv-portfoliocms-dev"  # Must be globally unique
$SQL_ADMIN_PASSWORD="YourSecurePassword123!"  # Use the password generated above

# Create resource group (if not exists)
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Key Vault
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-rbac-authorization false \
  --enabled-for-deployment true \
  --enabled-for-template-deployment true

# Store SQL admin password in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --value $SQL_ADMIN_PASSWORD

# Verify the secret was created
az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv
```

#### Step 3.3: Grant Your Account Access to Key Vault (for Terraform)

```bash
# Get your Azure AD user object ID
$USER_OBJECT_ID=$(az ad signed-in-user show --query id --output tsv)

# Grant yourself access to manage secrets
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id $USER_OBJECT_ID \
  --secret-permissions get list set delete purge recover
```

#### Step 3.4: Retrieve Password for Terraform Variables

```bash
# Retrieve the password from Key Vault for use in Terraform
$SQL_PASSWORD=$(az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv)

# Set as environment variable for Terraform
$env:TF_VAR_sql_admin_password=$SQL_PASSWORD

# Verify it's set
echo $env:TF_VAR_sql_admin_password
```

### 4. Deploy Infrastructure with Terraform/OpenTofu

After completing the manual Key Vault setup above:

```bash
# Navigate to infrastructure directory
cd infrastructure

# Initialize tofu
tofu init

# Retrieve password from Key Vault and set as environment variable
$SQL_PASSWORD=$(az keyvault secret show \
  --vault-name "kv-portfoliocms-dev" \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv)
$env:TF_VAR_sql_admin_password=$SQL_PASSWORD

# Plan deployment (development environment)
tofu plan -var-file="environments/development/terraform.tfvars"

# Apply deployment
tofu apply -var-file="environments/development/terraform.tfvars"
```

### 5. Configure Managed Identity Access to Key Vault

After Terraform creates the App Services, grant them access to Key Vault:

```bash
# Get the App Service managed identity principal IDs
$API_PRINCIPAL_ID=$(az webapp identity show \
  --name "app-portfoliocms-api-dev" \
  --resource-group "rg-portfoliocms-dev" \
  --query principalId \
  --output tsv)

$FRONTEND_PRINCIPAL_ID=$(az webapp identity show \
  --name "app-portfoliocms-frontend-dev" \
  --resource-group "rg-portfoliocms-dev" \
  --query principalId \
  --output tsv)

# Grant API App Service access to Key Vault secrets
az keyvault set-policy \
  --name "kv-portfoliocms-dev" \
  --object-id $API_PRINCIPAL_ID \
  --secret-permissions get list

# Grant Frontend App Service access to Key Vault secrets
az keyvault set-policy \
  --name "kv-portfoliocms-dev" \
  --object-id $FRONTEND_PRINCIPAL_ID \
  --secret-permissions get list

# Verify access policies
az keyvault show \
  --name "kv-portfoliocms-dev" \
  --query "properties.accessPolicies[].objectId" \
  --output table
```

## Environment Management

The infrastructure supports three environments:

- **Development** (`environments/development/`)
- **Staging** (`environments/staging/`)
- **Production** (`environments/production/`)

### Deploy to Specific Environment

```bash
# Development
tofu plan -var-file="environments/development/terraform.tfvars"
tofu apply -var-file="environments/development/terraform.tfvars"

# Staging
tofu plan -var-file="environments/staging/terraform.tfvars"
tofu apply -var-file="environments/staging/terraform.tfvars"

# Production
tofu plan -var-file="environments/production/terraform.tfvars"
tofu apply -var-file="environments/production/terraform.tfvars"
```

## Configuration

### Required Variables

⚠️ **SECURITY NOTE**: The SQL admin password should NEVER be stored in `terraform.tfvars` files or committed to version control. Always retrieve it from Azure Key Vault or use environment variables.

#### For Local Development

```bash
# Retrieve password from Key Vault and set as environment variable
$SQL_PASSWORD=$(az keyvault secret show \
  --vault-name "kv-portfoliocms-dev" \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv)
$env:TF_VAR_sql_admin_password=$SQL_PASSWORD
```

#### For CI/CD Pipelines

Set these environment variables in your GitHub Actions secrets or CI/CD system:

```bash
# Azure authentication
export ARM_CLIENT_ID="your-service-principal-client-id"
export ARM_CLIENT_SECRET="your-service-principal-client-secret"
export ARM_SUBSCRIPTION_ID="your-subscription-id"
export ARM_TENANT_ID="your-tenant-id"

# SQL password retrieved from Key Vault in pipeline
export TF_VAR_sql_admin_password="${{ secrets.SQL_ADMIN_PASSWORD }}"
```

### GitHub Actions Secrets Setup

For CI/CD pipelines, configure these secrets in your GitHub repository:

1. Go to repository Settings → Secrets and variables → Actions
2. Add the following secrets:
   - `AZURE_CREDENTIALS`: Service principal JSON for Azure authentication
   - `SQL_ADMIN_PASSWORD`: Retrieved from Key Vault (for Terraform)
   - `KEY_VAULT_NAME`: Name of your Key Vault (e.g., "kv-portfoliocms-dev")

## Resources Created

The tofu configuration creates the following Azure resources:

### Core Infrastructure
- **Resource Group** - Container for all resources
- **App Service Plan** - Hosting plan for web applications (B1 tier for dev)
- **App Services** (2) - Frontend and API applications with system-assigned managed identity
- **SQL Server** - Database server
- **SQL Database** - Application database (Free tier: 32 MB storage, 7-day backup retention)

### Storage & Security
- **Storage Account** - Media file storage
- **Storage Container** - Blob container for media files
- **Key Vault** - Secrets management (stores database passwords, connection strings)
- **Managed Identity** - System-assigned identity for App Services to access Key Vault

### Monitoring & Observability
- **Log Analytics Workspace** - Centralized logging
- **Application Insights** - Application performance monitoring

### Important Notes on Azure SQL Database Free Tier

- **Storage Limit**: 32 MB maximum database size
- **Backup Retention**: 7 days (vs 35 days for paid tiers)
- **Performance**: Suitable for development and small-scale testing
- **Limitations**: No geo-replication, no elastic pools
- **Cost**: Completely free (no charges)
- **Upgrade Path**: Can easily upgrade to Basic/Standard tier by changing SKU

## Outputs

After deployment, tofu provides the following outputs:

- `frontend_app_url` - URL of the frontend application
- `api_app_url` - URL of the API application
- `sql_server_fqdn` - SQL Server fully qualified domain name
- `key_vault_uri` - Key Vault URI for application configuration
- `application_insights_connection_string` - Application Insights connection string

## Security Considerations

1. **Secrets Management**: 
   - All sensitive values (database passwords, connection strings, API keys) are stored in Azure Key Vault
   - Database password is manually created in Key Vault BEFORE Terraform deployment
   - Never store passwords in Terraform files or version control
   - Applications retrieve secrets at runtime using managed identity

2. **Managed Identity**: 
   - App Services use system-assigned managed identity to access Key Vault
   - No credentials stored in application configuration
   - Access policies grant only necessary permissions (Get, List secrets)

3. **Network Security**: 
   - SQL Server firewall rules restrict access to Azure services only
   - Key Vault network rules can be configured for additional security

4. **HTTPS Only**: 
   - All App Services enforce HTTPS
   - TLS 1.2 minimum version enforced

5. **Credential Rotation**:
   - Database passwords can be rotated in Key Vault
   - Applications automatically pick up new values on restart
   - No code changes required for credential rotation

## Cost Optimization

### Development Environment
- Basic App Service Plan (B1) - ~$13/month
- **Azure SQL Database Free tier** - $0/month (32 MB storage)
- Standard LRS storage replication - ~$0.02/GB/month
- Key Vault - $0.03 per 10,000 operations
- **Estimated Monthly Cost**: ~$15-20/month

### Staging Environment
- Basic App Service Plan (B1) - ~$13/month
- Basic SQL Database (2 GB) - ~$5/month
- Standard LRS storage replication - ~$0.02/GB/month
- **Estimated Monthly Cost**: ~$20-25/month

### Production Environment
- Premium App Service Plan (P1v2) with auto-scaling - ~$75/month
- Standard SQL Database (10 GB) with geo-replication - ~$30/month
- GRS storage replication for high availability - ~$0.05/GB/month
- **Estimated Monthly Cost**: ~$110-150/month

### Cost Saving Tips
1. Use Free tier SQL Database for development (saves ~$5/month)
2. Stop App Services when not in use (dev/staging environments)
3. Use Azure Cost Management alerts to monitor spending
4. Consider Azure Dev/Test pricing for non-production environments

## Monitoring and Alerting

The infrastructure includes comprehensive monitoring:

- **Application Insights** for application telemetry
- **Log Analytics** for centralized logging
- **Azure Monitor** for infrastructure metrics
- **Grafana Cloud** integration - See [GRAFANA_CLOUD_SETUP.md](./GRAFANA_CLOUD_SETUP.md) for setup instructions

### Monitoring Features

- **Automated Alerts**: Response time, error rate, DTU usage, storage availability
- **Web Tests**: Synthetic monitoring for frontend and API endpoints
- **Custom Dashboards**: Pre-configured dashboards in Application Insights
- **Action Groups**: Email notifications for critical alerts

For detailed Grafana Cloud integration, including dashboard templates and alerting rules, see [GRAFANA_CLOUD_SETUP.md](./GRAFANA_CLOUD_SETUP.md).

## Backup and Disaster Recovery

- **SQL Database**: Automated backups with 35-day retention
- **Storage Account**: Geo-redundant storage in production
- **Infrastructure**: Version-controlled tofu state

## Key Vault Secret Management

### Storing Additional Secrets

As your application grows, you may need to store additional secrets in Key Vault:

```bash
# Store API keys
az keyvault secret set \
  --vault-name "kv-portfoliocms-dev" \
  --name "OpenAI-ApiKey" \
  --value "your-api-key-here"

# Store connection strings
az keyvault secret set \
  --vault-name "kv-portfoliocms-dev" \
  --name "StorageConnectionString" \
  --value "DefaultEndpointsProtocol=https;AccountName=..."

# Store JWT signing keys
az keyvault secret set \
  --vault-name "kv-portfoliocms-dev" \
  --name "JwtSigningKey" \
  --value "your-secure-signing-key"
```

### Retrieving Secrets in Application Code

```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault configuration
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

// Access secrets like regular configuration
var sqlPassword = builder.Configuration["SqlAdminPassword"];
var connectionString = builder.Configuration["SqlConnectionString"];
var apiKey = builder.Configuration["OpenAI-ApiKey"];
```

### Rotating Database Password

To rotate the database password without downtime:

```bash
# 1. Generate new password
$NEW_PASSWORD = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 16 | ForEach-Object {[char]$_})

# 2. Update SQL Server password
az sql server update \
  --resource-group "rg-portfoliocms-dev" \
  --name "sql-portfoliocms-dev" \
  --admin-password $NEW_PASSWORD

# 3. Update Key Vault secret
az keyvault secret set \
  --vault-name "kv-portfoliocms-dev" \
  --name "SqlAdminPassword" \
  --value $NEW_PASSWORD

# 4. Update connection string in Key Vault
$CONNECTION_STRING="Server=tcp:sql-portfoliocms-dev.database.windows.net,1433;Initial Catalog=sqldb-portfoliocms;User ID=sqladmin;Password=$NEW_PASSWORD;Encrypt=True;"
az keyvault secret set \
  --vault-name "kv-portfoliocms-dev" \
  --name "SqlConnectionString" \
  --value $CONNECTION_STRING

# 5. Restart App Services to pick up new password
az webapp restart --name "app-portfoliocms-api-dev" --resource-group "rg-portfoliocms-dev"
az webapp restart --name "app-portfoliocms-frontend-dev" --resource-group "rg-portfoliocms-dev"
```

### Listing All Secrets

```bash
# List all secrets in Key Vault
az keyvault secret list \
  --vault-name "kv-portfoliocms-dev" \
  --query "[].{Name:name, Enabled:attributes.enabled}" \
  --output table

# Show specific secret value (use carefully)
az keyvault secret show \
  --vault-name "kv-portfoliocms-dev" \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv
```

## Troubleshooting

### Common Issues

1. **Key Vault Access Denied**
   - **Error**: "The user, group or application does not have secrets get permission"
   - **Solution**: Grant access policy to your account or managed identity
   ```bash
   az keyvault set-policy \
     --name "kv-portfoliocms-dev" \
     --object-id <your-object-id> \
     --secret-permissions get list
   ```

2. **SQL Database Size Limit Exceeded (Free Tier)**
   - **Error**: "Database size limit has been reached"
   - **Solution**: Free tier has 32 MB limit. Either:
     - Clean up old data
     - Upgrade to Basic tier ($5/month for 2 GB)
   ```bash
   az sql db update \
     --resource-group "rg-portfoliocms-dev" \
     --server "sql-portfoliocms-dev" \
     --name "sqldb-portfoliocms" \
     --service-objective Basic
   ```

3. **Managed Identity Not Working**
   - **Error**: "No managed identity found"
   - **Solution**: Ensure system-assigned identity is enabled
   ```bash
   az webapp identity assign \
     --name "app-portfoliocms-api-dev" \
     --resource-group "rg-portfoliocms-dev"
   ```

4. **Storage Account Name Conflicts**
   - Storage account names must be globally unique
   - Modify `storage_account_name` variable if deployment fails

5. **SQL Server Name Conflicts**
   - SQL server names must be globally unique
   - Modify `sql_server_name` variable if deployment fails

6. **Permission Issues**
   - Ensure your Azure account has Contributor role on the subscription
   - For CI/CD, ensure service principal has appropriate permissions

7. **Connection String Not Found**
   - **Error**: "Configuration 'SqlConnectionString' not found"
   - **Solution**: Verify Key Vault secret exists and App Service has access
   ```bash
   # Check if secret exists
   az keyvault secret show \
     --vault-name "kv-portfoliocms-dev" \
     --name "SqlConnectionString"
   
   # Check App Service identity
   az webapp identity show \
     --name "app-portfoliocms-api-dev" \
     --resource-group "rg-portfoliocms-dev"
   ```

### Useful Commands

```bash
# Check tofu state
tofu show

# List all resources
tofu state list

# Get specific resource information
tofu state show azurerm_linux_web_app.frontend

# Destroy infrastructure (be careful!)
tofu destroy -var-file="environments/development/terraform.tfvars"
```

## CI/CD Integration

This infrastructure is designed to work with GitHub Actions. See `.github/workflows/` for deployment pipelines.

The CI/CD pipeline:
1. Validates tofu configuration
2. Plans infrastructure changes
3. Applies changes to staging environment
4. Runs tests against staging
5. Deploys to production (manual approval required)

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review Azure resource logs in the Azure Portal
3. Check tofu state and plan output
4. Consult Azure documentation for specific resource issues