# Manual Azure Setup Guide

This guide provides step-by-step instructions for manually setting up Azure resources before running Terraform/OpenTofu. These manual steps are required for security best practices and initial infrastructure bootstrapping.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Initial Azure Setup](#initial-azure-setup)
3. [Key Vault Secret Initialization](#key-vault-secret-initialization)
4. [Service Principal Setup for CI/CD](#service-principal-setup-for-cicd)
5. [Terraform State Storage Setup](#terraform-state-storage-setup)
6. [SSL Certificate Setup (Optional)](#ssl-certificate-setup-optional)
7. [Verification Steps](#verification-steps)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

Before starting, ensure you have:

1. **Azure CLI** installed and configured
   ```bash
   # Install Azure CLI (Windows)
   winget install Microsoft.AzureCLI
   
   # Verify installation
   az --version
   ```

2. **Active Azure Subscription** with appropriate permissions
   - Contributor or Owner role on the subscription
   - Ability to create service principals

3. **PowerShell** or **Bash** terminal

4. **Terraform/OpenTofu** installed
   ```bash
   # Install Terraform (Windows)
   winget install Hashicorp.Terraform
   
   # Verify installation
   terraform --version
   ```

---

## Initial Azure Setup

### Step 1: Login to Azure

```bash
# Login to Azure
az login

# List available subscriptions
az account list --output table

# Set the active subscription
az account set --subscription "your-subscription-id"

# Verify the active subscription
az account show --output table
```

### Step 2: Set Environment Variables

```powershell
# PowerShell - Set variables for your environment
$ENVIRONMENT = "development"  # or "staging" or "production"
$LOCATION = "eastus"
$RESOURCE_GROUP = "rg-portfoliocms-$ENVIRONMENT"
$KEY_VAULT_NAME = "kv-portfoliocms-$ENVIRONMENT"  # Must be globally unique (3-24 chars)
$SQL_SERVER_NAME = "sql-portfoliocms-$ENVIRONMENT"  # Must be globally unique
$STORAGE_ACCOUNT_NAME = "stportfoliocms$ENVIRONMENT"  # Must be globally unique (3-24 chars, lowercase, no hyphens)

# Display variables for verification
Write-Host "Environment: $ENVIRONMENT"
Write-Host "Location: $LOCATION"
Write-Host "Resource Group: $RESOURCE_GROUP"
Write-Host "Key Vault: $KEY_VAULT_NAME"
Write-Host "SQL Server: $SQL_SERVER_NAME"
Write-Host "Storage Account: $STORAGE_ACCOUNT_NAME"
```

```bash
# Bash - Set variables for your environment
ENVIRONMENT="development"  # or "staging" or "production"
LOCATION="eastus"
RESOURCE_GROUP="rg-portfoliocms-$ENVIRONMENT"
KEY_VAULT_NAME="kv-portfoliocms-$ENVIRONMENT"  # Must be globally unique (3-24 chars)
SQL_SERVER_NAME="sql-portfoliocms-$ENVIRONMENT"  # Must be globally unique
STORAGE_ACCOUNT_NAME="stportfoliocms$ENVIRONMENT"  # Must be globally unique (3-24 chars, lowercase, no hyphens)

# Display variables for verification
echo "Environment: $ENVIRONMENT"
echo "Location: $LOCATION"
echo "Resource Group: $RESOURCE_GROUP"
echo "Key Vault: $KEY_VAULT_NAME"
echo "SQL Server: $SQL_SERVER_NAME"
echo "Storage Account: $STORAGE_ACCOUNT_NAME"
```

---

## Key Vault Secret Initialization

### Step 3: Create Resource Group

```bash
# Create resource group for the environment
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION \
  --tags Project=PortfolioCMS Environment=$ENVIRONMENT ManagedBy=Manual

# Verify resource group creation
az group show --name $RESOURCE_GROUP --output table
```

### Step 4: Create Key Vault

```bash
# Create Key Vault
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-rbac-authorization false \
  --enabled-for-deployment true \
  --enabled-for-template-deployment true \
  --enabled-for-disk-encryption true \
  --sku standard \
  --tags Project=PortfolioCMS Environment=$ENVIRONMENT

# Verify Key Vault creation
az keyvault show --name $KEY_VAULT_NAME --output table
```

### Step 5: Generate Secure Database Password

```powershell
# PowerShell - Generate a strong random password
$SQL_ADMIN_PASSWORD = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 20 | ForEach-Object {[char]$_})
$SQL_ADMIN_PASSWORD += "!@#"  # Add special characters to meet Azure SQL requirements

# Display password (save this securely!)
Write-Host "Generated SQL Admin Password: $SQL_ADMIN_PASSWORD"
Write-Host "IMPORTANT: Save this password securely! You will need it for Terraform."
```

```bash
# Bash - Generate a strong random password
SQL_ADMIN_PASSWORD=$(openssl rand -base64 16 | tr -d "=+/" | cut -c1-20)
SQL_ADMIN_PASSWORD="${SQL_ADMIN_PASSWORD}!@#"  # Add special characters

# Display password (save this securely!)
echo "Generated SQL Admin Password: $SQL_ADMIN_PASSWORD"
echo "IMPORTANT: Save this password securely! You will need it for Terraform."
```

**⚠️ CRITICAL: Save this password in a secure location (password manager, secure note, etc.)**

### Step 6: Store Database Password in Key Vault

```bash
# Store SQL admin password in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --value "$SQL_ADMIN_PASSWORD" \
  --description "SQL Server administrator password for $ENVIRONMENT environment"

# Verify secret was created (this will show the secret value - be careful!)
az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --query "{Name:name, Enabled:attributes.enabled, Created:attributes.created}" \
  --output table
```

### Step 7: Grant Your Account Access to Key Vault

```bash
# Get your Azure AD user object ID
USER_OBJECT_ID=$(az ad signed-in-user show --query id --output tsv)

# Grant yourself access to manage secrets
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id $USER_OBJECT_ID \
  --secret-permissions get list set delete purge recover backup restore

# Verify access policy
az keyvault show \
  --name $KEY_VAULT_NAME \
  --query "properties.accessPolicies[?objectId=='$USER_OBJECT_ID']" \
  --output table
```

---

## Service Principal Setup for CI/CD

### Step 8: Create Service Principal for Terraform

```bash
# Get your subscription ID
SUBSCRIPTION_ID=$(az account show --query id --output tsv)

# Create service principal with Contributor role
az ad sp create-for-rbac \
  --name "sp-portfoliocms-terraform-$ENVIRONMENT" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP \
  --sdk-auth

# IMPORTANT: Save the output JSON securely! You'll need it for GitHub Actions.
# The output will look like:
# {
#   "clientId": "...",
#   "clientSecret": "...",
#   "subscriptionId": "...",
#   "tenantId": "...",
#   ...
# }
```

**⚠️ CRITICAL: Save the entire JSON output in a secure location. You'll need it for:**
- GitHub Actions secret: `AZURE_CREDENTIALS`
- Terraform environment variables

### Step 9: Grant Service Principal Access to Key Vault

```bash
# Get the service principal object ID
SP_OBJECT_ID=$(az ad sp list --display-name "sp-portfoliocms-terraform-$ENVIRONMENT" --query "[0].id" --output tsv)

# Grant service principal access to Key Vault secrets
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id $SP_OBJECT_ID \
  --secret-permissions get list set delete

# Verify access policy
az keyvault show \
  --name $KEY_VAULT_NAME \
  --query "properties.accessPolicies[?objectId=='$SP_OBJECT_ID']" \
  --output table
```

---

## Terraform State Storage Setup

### Step 10: Create Storage Account for Terraform State

```bash
# Create resource group for Terraform state (shared across environments)
az group create \
  --name "rg-portfoliocms-tfstate" \
  --location $LOCATION \
  --tags Project=PortfolioCMS Purpose=TerraformState

# Create storage account for Terraform state
az storage account create \
  --name "stportfoliocmstfstate" \
  --resource-group "rg-portfoliocms-tfstate" \
  --location $LOCATION \
  --sku Standard_LRS \
  --encryption-services blob \
  --https-only true \
  --min-tls-version TLS1_2 \
  --tags Project=PortfolioCMS Purpose=TerraformState

# Get storage account key
STORAGE_ACCOUNT_KEY=$(az storage account keys list \
  --resource-group "rg-portfoliocms-tfstate" \
  --account-name "stportfoliocmstfstate" \
  --query "[0].value" \
  --output tsv)

# Create blob container for Terraform state
az storage container create \
  --name "tfstate" \
  --account-name "stportfoliocmstfstate" \
  --account-key $STORAGE_ACCOUNT_KEY \
  --public-access off

# Verify container creation
az storage container show \
  --name "tfstate" \
  --account-name "stportfoliocmstfstate" \
  --account-key $STORAGE_ACCOUNT_KEY \
  --output table
```

### Step 11: Enable Blob Versioning and Soft Delete

```bash
# Enable blob versioning for state file protection
az storage account blob-service-properties update \
  --account-name "stportfoliocmstfstate" \
  --resource-group "rg-portfoliocms-tfstate" \
  --enable-versioning true

# Enable soft delete for blobs (7 days retention)
az storage account blob-service-properties update \
  --account-name "stportfoliocmstfstate" \
  --resource-group "rg-portfoliocms-tfstate" \
  --enable-delete-retention true \
  --delete-retention-days 7

# Enable soft delete for containers (7 days retention)
az storage account blob-service-properties update \
  --account-name "stportfoliocmstfstate" \
  --resource-group "rg-portfoliocms-tfstate" \
  --enable-container-delete-retention true \
  --container-delete-retention-days 7
```

---

## SSL Certificate Setup (Optional)

### Step 12: Generate Self-Signed Certificate for Development

```powershell
# PowerShell - Generate self-signed certificate for development/testing
$cert = New-SelfSignedCertificate `
  -Subject "CN=portfoliocms-$ENVIRONMENT.local" `
  -DnsName "portfoliocms-$ENVIRONMENT.local", "*.portfoliocms-$ENVIRONMENT.local" `
  -KeyAlgorithm RSA `
  -KeyLength 2048 `
  -NotBefore (Get-Date) `
  -NotAfter (Get-Date).AddYears(2) `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -FriendlyName "PortfolioCMS $ENVIRONMENT SSL Certificate" `
  -KeyUsageProperty All `
  -KeyUsage CertSign, CRLSign, DigitalSignature

# Export certificate to PFX file
$certPassword = ConvertTo-SecureString -String "YourCertPassword123!" -Force -AsPlainText
Export-PfxCertificate `
  -Cert $cert `
  -FilePath ".\portfoliocms-$ENVIRONMENT.pfx" `
  -Password $certPassword

Write-Host "Certificate exported to: portfoliocms-$ENVIRONMENT.pfx"
Write-Host "Certificate password: YourCertPassword123!"
```

### Step 13: Store SSL Certificate in Key Vault

```bash
# Convert PFX to base64 for Terraform
CERT_BASE64=$(cat portfoliocms-$ENVIRONMENT.pfx | base64 -w 0)

# Store certificate data in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SslCertificateData" \
  --value "$CERT_BASE64" \
  --description "SSL certificate for Application Gateway"

# Store certificate password in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SslCertificatePassword" \
  --value "YourCertPassword123!" \
  --description "SSL certificate password"
```

**Note:** For production, use a certificate from a trusted Certificate Authority (Let's Encrypt, DigiCert, etc.)

---

## Verification Steps

### Step 14: Verify All Manual Setup

```bash
# 1. Verify Resource Group
echo "=== Resource Group ==="
az group show --name $RESOURCE_GROUP --output table

# 2. Verify Key Vault
echo "=== Key Vault ==="
az keyvault show --name $KEY_VAULT_NAME --output table

# 3. Verify Key Vault Secrets
echo "=== Key Vault Secrets ==="
az keyvault secret list --vault-name $KEY_VAULT_NAME --output table

# 4. Verify Key Vault Access Policies
echo "=== Key Vault Access Policies ==="
az keyvault show \
  --name $KEY_VAULT_NAME \
  --query "properties.accessPolicies[].{ObjectId:objectId, Permissions:permissions.secrets}" \
  --output table

# 5. Verify Terraform State Storage
echo "=== Terraform State Storage ==="
az storage account show \
  --name "stportfoliocmstfstate" \
  --resource-group "rg-portfoliocms-tfstate" \
  --output table

# 6. Verify Terraform State Container
echo "=== Terraform State Container ==="
STORAGE_ACCOUNT_KEY=$(az storage account keys list \
  --resource-group "rg-portfoliocms-tfstate" \
  --account-name "stportfoliocmstfstate" \
  --query "[0].value" \
  --output tsv)

az storage container show \
  --name "tfstate" \
  --account-name "stportfoliocmstfstate" \
  --account-key $STORAGE_ACCOUNT_KEY \
  --output table

# 7. Test retrieving SQL password from Key Vault
echo "=== Test SQL Password Retrieval ==="
az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv | wc -c
echo "Password length retrieved successfully"
```

### Step 15: Test Terraform Initialization

```bash
# Navigate to infrastructure directory
cd infrastructure

# Set environment variable for SQL password
export TF_VAR_sql_admin_password=$(az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv)

# Initialize Terraform
terraform init

# Validate Terraform configuration
terraform validate

# Plan Terraform deployment (dry run)
terraform plan -var-file="environments/$ENVIRONMENT/terraform.tfvars"

echo "If the plan succeeds, manual setup is complete!"
```

---

## Troubleshooting

### Issue 1: Key Vault Name Already Exists

**Error:** `The vault name 'kv-portfoliocms-dev' is already in use.`

**Solution:**
```bash
# Key Vault names must be globally unique. Try adding a suffix:
KEY_VAULT_NAME="kv-portfoliocms-$ENVIRONMENT-$(date +%s)"
echo "New Key Vault name: $KEY_VAULT_NAME"

# Recreate Key Vault with new name
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION
```

### Issue 2: Access Denied to Key Vault

**Error:** `The user, group or application does not have secrets get permission`

**Solution:**
```bash
# Grant yourself access
USER_OBJECT_ID=$(az ad signed-in-user show --query id --output tsv)
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id $USER_OBJECT_ID \
  --secret-permissions get list set delete
```

### Issue 3: Storage Account Name Already Exists

**Error:** `The storage account named 'stportfoliocmsdev' is already taken.`

**Solution:**
```bash
# Storage account names must be globally unique. Try adding a suffix:
STORAGE_ACCOUNT_NAME="stportfoliocms$ENVIRONMENT$(date +%s | tail -c 5)"
echo "New storage account name: $STORAGE_ACCOUNT_NAME"

# Recreate storage account with new name
az storage account create \
  --name $STORAGE_ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS
```

### Issue 4: Service Principal Creation Failed

**Error:** `Insufficient privileges to complete the operation`

**Solution:**
- You need "Application Administrator" or "Global Administrator" role in Azure AD
- Contact your Azure AD administrator to create the service principal
- Alternatively, use your personal credentials for local development

### Issue 5: Terraform Backend Initialization Failed

**Error:** `Error: Failed to get existing workspaces: storage: service returned error`

**Solution:**
```bash
# Verify storage account exists and you have access
az storage account show \
  --name "stportfoliocmstfstate" \
  --resource-group "rg-portfoliocms-tfstate"

# Verify container exists
STORAGE_ACCOUNT_KEY=$(az storage account keys list \
  --resource-group "rg-portfoliocms-tfstate" \
  --account-name "stportfoliocmstfstate" \
  --query "[0].value" \
  --output tsv)

az storage container show \
  --name "tfstate" \
  --account-name "stportfoliocmstfstate" \
  --account-key $STORAGE_ACCOUNT_KEY
```

---

## Summary Checklist

Before running Terraform, ensure you have completed:

- [ ] Logged in to Azure CLI
- [ ] Set active subscription
- [ ] Created resource group for environment
- [ ] Created Key Vault
- [ ] Generated and stored SQL admin password in Key Vault
- [ ] Granted your account access to Key Vault
- [ ] Created service principal for Terraform
- [ ] Granted service principal access to Key Vault
- [ ] Created Terraform state storage account
- [ ] Created Terraform state blob container
- [ ] Enabled blob versioning and soft delete
- [ ] (Optional) Generated and stored SSL certificate
- [ ] Verified all resources are accessible
- [ ] Successfully ran `terraform init` and `terraform validate`

---

## Next Steps

After completing this manual setup:

1. **Update Terraform Variables**: Edit `infrastructure/environments/{environment}/terraform.tfvars` with your resource names
2. **Run Terraform Plan**: `terraform plan -var-file="environments/{environment}/terraform.tfvars"`
3. **Apply Infrastructure**: `terraform apply -var-file="environments/{environment}/terraform.tfvars"`
4. **Configure GitHub Actions**: Add secrets to GitHub repository for CI/CD
5. **Deploy Applications**: Deploy Frontend and API projects to Azure App Services

---

## Security Best Practices

1. **Never commit secrets to version control**
   - Use `.gitignore` to exclude `.tfvars` files with sensitive data
   - Always retrieve secrets from Key Vault

2. **Rotate credentials regularly**
   - Rotate SQL passwords every 90 days
   - Rotate service principal secrets every 180 days

3. **Use separate Key Vaults per environment**
   - Development, Staging, and Production should have separate Key Vaults
   - Never share production secrets with non-production environments

4. **Enable audit logging**
   - Enable diagnostic settings on Key Vault
   - Monitor access logs for suspicious activity

5. **Use managed identities when possible**
   - App Services should use managed identity to access Key Vault
   - Avoid storing credentials in application configuration

---

## Additional Resources

- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)
- [Azure Security Best Practices](https://docs.microsoft.com/en-us/azure/security/fundamentals/best-practices-and-patterns)
