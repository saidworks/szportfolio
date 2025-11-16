# Azure Security Services Setup Guide

This guide provides step-by-step instructions for setting up Azure security services for the Portfolio CMS application, including Azure Key Vault, Managed Identity, Azure Active Directory, and Application Gateway WAF.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Azure Key Vault Setup](#azure-key-vault-setup)
3. [Managed Identity Configuration](#managed-identity-configuration)
4. [Microsoft Entra ID Integration](#microsoft-entra-id-integration)
5. [Application Gateway WAF Configuration](#application-gateway-waf-configuration)
6. [Security Validation](#security-validation)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

Before starting, ensure you have:

- Azure CLI installed and configured (`az --version`)
- Azure subscription with appropriate permissions
- Terraform/OpenTofu installed (`terraform --version`)
- Access to Azure Portal
- PowerShell or Bash terminal

### Login to Azure

```bash
# Login to Azure
az login

# Set the subscription (if you have multiple)
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Verify current subscription
az account show
```

## Azure Key Vault Setup

### Step 1: Create Key Vault (via Terraform)

The Key Vault is created automatically by Terraform, but you need to manually set the initial secrets for security reasons.

```bash
# Navigate to infrastructure directory
cd infrastructure

# Initialize Terraform
terraform init

# Plan the deployment
terraform plan -out=tfplan

# Apply the deployment
terraform apply tfplan
```

### Step 2: Manually Set Initial Secrets

**IMPORTANT**: Never store sensitive passwords in Terraform state. Set them manually after Key Vault creation.

```bash
# Set variables
RESOURCE_GROUP="rg-portfoliocms"
KEY_VAULT_NAME="kv-portfoliocms"
SQL_ADMIN_PASSWORD="YourSecurePassword123!"

# Store SQL admin password in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --value "$SQL_ADMIN_PASSWORD"

# Generate and store JWT secret key (32+ characters)
JWT_SECRET=$(openssl rand -base64 32)
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "JwtSecretKey" \
  --value "$JWT_SECRET"

# Verify secrets are stored
az keyvault secret list --vault-name $KEY_VAULT_NAME --output table
```

### Step 3: Store Connection Strings

```bash
# Get SQL Server FQDN
SQL_SERVER_FQDN=$(az sql server show \
  --resource-group $RESOURCE_GROUP \
  --name "sql-portfoliocms" \
  --query "fullyQualifiedDomainName" \
  --output tsv)

# Create SQL connection string
SQL_CONNECTION_STRING="Server=tcp:${SQL_SERVER_FQDN},1433;Initial Catalog=sqldb-portfoliocms;Persist Security Info=False;User ID=sqladmin;Password=${SQL_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Store SQL connection string
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlConnectionString" \
  --value "$SQL_CONNECTION_STRING"

# Get Storage Account connection string
STORAGE_CONNECTION_STRING=$(az storage account show-connection-string \
  --resource-group $RESOURCE_GROUP \
  --name "stportfoliocmsmedia" \
  --query "connectionString" \
  --output tsv)

# Store Storage connection string
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "StorageConnectionString" \
  --value "$STORAGE_CONNECTION_STRING"
```

### Step 4: Verify Key Vault Access Policies

```bash
# List access policies
az keyvault show \
  --resource-group $RESOURCE_GROUP \
  --name $KEY_VAULT_NAME \
  --query "properties.accessPolicies" \
  --output table

# Verify managed identities have access
az keyvault access-policy list \
  --name $KEY_VAULT_NAME \
  --output table
```

## Managed Identity Configuration

### Step 1: Verify Managed Identities

Managed identities are automatically created by Terraform for App Services.

```bash
# Get Frontend App managed identity
FRONTEND_IDENTITY=$(az webapp identity show \
  --resource-group $RESOURCE_GROUP \
  --name "app-portfoliocms-frontend" \
  --query "principalId" \
  --output tsv)

echo "Frontend Identity: $FRONTEND_IDENTITY"

# Get API App managed identity
API_IDENTITY=$(az webapp identity show \
  --resource-group $RESOURCE_GROUP \
  --name "app-portfoliocms-api" \
  --query "principalId" \
  --output tsv)

echo "API Identity: $API_IDENTITY"
```

### Step 2: Grant Key Vault Access (Already configured by Terraform)

The access policies are automatically configured, but you can verify:

```bash
# Verify Frontend has Key Vault access
az keyvault access-policy show \
  --name $KEY_VAULT_NAME \
  --object-id $FRONTEND_IDENTITY

# Verify API has Key Vault access
az keyvault access-policy show \
  --name $KEY_VAULT_NAME \
  --object-id $API_IDENTITY
```

### Step 3: Test Managed Identity Access

```bash
# Test retrieving a secret using managed identity (from App Service)
# This command should be run from within the App Service environment

# For local testing, use Azure CLI credentials
az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv
```

## Microsoft Entra ID Integration

Microsoft Entra ID (formerly Azure Active Directory) provides enterprise-grade authentication and authorization.

### Step 1: Register Application in Microsoft Entra ID

```bash
# Create App Registration for API
API_APP_ID=$(az ad app create \
  --display-name "PortfolioCMS API" \
  --sign-in-audience "AzureADMyOrg" \
  --query "appId" \
  --output tsv)

echo "API App ID: $API_APP_ID"

# Create App Registration for Frontend
FRONTEND_APP_ID=$(az ad app create \
  --display-name "PortfolioCMS Frontend" \
  --sign-in-audience "AzureADMyOrg" \
  --query "appId" \
  --output tsv)

echo "Frontend App ID: $FRONTEND_APP_ID"
```

### Step 2: Configure API Permissions

```bash
# Expose API scope
az ad app update \
  --id $API_APP_ID \
  --identifier-uris "api://$API_APP_ID"

# Add API scope for user impersonation
az ad app permission add \
  --id $FRONTEND_APP_ID \
  --api $API_APP_ID \
  --api-permissions "user_impersonation=Scope"

# Grant admin consent
az ad app permission admin-consent --id $FRONTEND_APP_ID
```

### Step 3: Create Service Principal

```bash
# Create service principal for API
az ad sp create --id $API_APP_ID

# Create service principal for Frontend
az ad sp create --id $FRONTEND_APP_ID
```

### Step 4: Configure App Roles in Entra ID

```bash
# Create Admin role for API app
az ad app update --id $API_APP_ID --app-roles '[
  {
    "allowedMemberTypes": ["User"],
    "description": "Administrators can manage all content",
    "displayName": "Admin",
    "id": "'$(uuidgen)'",
    "isEnabled": true,
    "value": "Admin"
  },
  {
    "allowedMemberTypes": ["User"],
    "description": "Viewers can read content",
    "displayName": "Viewer",
    "id": "'$(uuidgen)'",
    "isEnabled": true,
    "value": "Viewer"
  }
]'
```

### Step 5: Store Entra ID Configuration in Key Vault

```bash
# Get Tenant ID
TENANT_ID=$(az account show --query "tenantId" --output tsv)

# Store Entra ID configuration
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "EntraId--TenantId" \
  --value "$TENANT_ID"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "EntraId--ClientId" \
  --value "$API_APP_ID"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "EntraId--FrontendClientId" \
  --value "$FRONTEND_APP_ID"

# Also store with legacy AzureAd prefix for backward compatibility
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "AzureAd--TenantId" \
  --value "$TENANT_ID"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "AzureAd--ClientId" \
  --value "$API_APP_ID"
```

### Step 6: Update Application Configuration

Update `appsettings.json` to use Microsoft Entra ID (optional - currently using JWT):

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

## Application Gateway WAF Configuration

### Step 1: Enable Application Gateway (Optional)

Application Gateway with WAF is optional and recommended for production.

```bash
# Update terraform.tfvars to enable Application Gateway
cat >> infrastructure/terraform.tfvars <<EOF
enable_application_gateway = true
waf_mode = "Detection"  # Use "Prevention" for production
EOF

# Apply Terraform changes
cd infrastructure
terraform plan -out=tfplan
terraform apply tfplan
```

### Step 2: Configure WAF Custom Rules

The WAF custom rules are defined in the Terraform module `modules/waf-policies/`. Key protections include:

- **Rate Limiting**: 100 requests per minute per IP
- **Suspicious User Agents**: Blocks known scanning tools
- **IP Blocking**: Blocks known malicious IPs
- **Geo-Filtering**: Allows only specific countries

### Step 3: Configure SSL Certificate

```bash
# Create self-signed certificate for testing (DO NOT use in production)
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes

# Convert to PFX format
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem -password pass:YourPassword

# Encode to base64
cat certificate.pfx | base64 > certificate.b64

# Store in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SslCertificateData" \
  --file certificate.b64

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SslCertificatePassword" \
  --value "YourPassword"
```

### Step 4: Monitor WAF Logs

```bash
# Enable diagnostic settings for Application Gateway
az monitor diagnostic-settings create \
  --resource "/subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Network/applicationGateways/agw-portfoliocms" \
  --name "waf-diagnostics" \
  --workspace "/subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.OperationalInsights/workspaces/law-portfoliocms" \
  --logs '[{"category": "ApplicationGatewayFirewallLog", "enabled": true}]'

# Query WAF logs
az monitor log-analytics query \
  --workspace "law-portfoliocms" \
  --analytics-query "AzureDiagnostics | where Category == 'ApplicationGatewayFirewallLog' | take 100"
```

## Security Validation

### Step 1: Validate Key Vault Access

```bash
# Test retrieving secrets from Key Vault
az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "SqlAdminPassword" \
  --query "value" \
  --output tsv

# Verify all required secrets exist
REQUIRED_SECRETS=("SqlAdminPassword" "SqlConnectionString" "JwtSecretKey" "StorageConnectionString")

for secret in "${REQUIRED_SECRETS[@]}"; do
  if az keyvault secret show --vault-name $KEY_VAULT_NAME --name "$secret" &> /dev/null; then
    echo "✓ Secret '$secret' exists"
  else
    echo "✗ Secret '$secret' is missing"
  fi
done
```

### Step 2: Validate Managed Identity

```bash
# Test managed identity from App Service
# SSH into App Service (requires App Service SSH enabled)
az webapp ssh --resource-group $RESOURCE_GROUP --name "app-portfoliocms-api"

# Inside App Service, test managed identity
curl 'http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://vault.azure.net' -H Metadata:true
```

### Step 3: Validate WAF Protection

```bash
# Test SQL injection protection
curl -X GET "https://YOUR_APP_GATEWAY_IP/api/v1/articles?id=1' OR '1'='1" -v

# Test XSS protection
curl -X POST "https://YOUR_APP_GATEWAY_IP/api/v1/comments" \
  -H "Content-Type: application/json" \
  -d '{"content": "<script>alert(\"XSS\")</script>"}' -v

# Check WAF logs for blocked requests
az monitor log-analytics query \
  --workspace "law-portfoliocms" \
  --analytics-query "AzureDiagnostics | where Category == 'ApplicationGatewayFirewallLog' and action_s == 'Blocked' | take 10"
```

### Step 4: Security Checklist

- [ ] Key Vault created and accessible
- [ ] All secrets stored in Key Vault (no hardcoded credentials)
- [ ] Managed identities configured for App Services
- [ ] Key Vault access policies granted to managed identities
- [ ] Azure AD app registrations created (if using AAD)
- [ ] Application Gateway WAF enabled (production)
- [ ] WAF custom rules configured
- [ ] SSL certificate configured
- [ ] Diagnostic logging enabled
- [ ] Security alerts configured

## Troubleshooting

### Issue: Cannot access Key Vault secrets

**Solution**:
```bash
# Check if managed identity has access
az keyvault access-policy list --name $KEY_VAULT_NAME

# Grant access if missing
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id $API_IDENTITY \
  --secret-permissions get list
```

### Issue: Managed identity not working

**Solution**:
```bash
# Verify managed identity is enabled
az webapp identity show \
  --resource-group $RESOURCE_GROUP \
  --name "app-portfoliocms-api"

# Enable if not present
az webapp identity assign \
  --resource-group $RESOURCE_GROUP \
  --name "app-portfoliocms-api"
```

### Issue: WAF blocking legitimate requests

**Solution**:
```bash
# Switch WAF to Detection mode
az network application-gateway waf-config set \
  --resource-group $RESOURCE_GROUP \
  --gateway-name "agw-portfoliocms" \
  --enabled true \
  --firewall-mode Detection

# Review WAF logs to identify false positives
az monitor log-analytics query \
  --workspace "law-portfoliocms" \
  --analytics-query "AzureDiagnostics | where Category == 'ApplicationGatewayFirewallLog' and action_s == 'Blocked' | project TimeGenerated, requestUri_s, ruleId_s, Message"
```

### Issue: SSL certificate errors

**Solution**:
```bash
# Verify certificate is valid
openssl x509 -in cert.pem -text -noout

# Check certificate expiration
openssl x509 -in cert.pem -noout -enddate

# Update certificate in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "SslCertificateData" \
  --file certificate.b64
```

## Additional Resources

- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Managed Identities for Azure Resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)
- [Azure Application Gateway WAF](https://docs.microsoft.com/en-us/azure/web-application-firewall/ag/ag-overview)
- [Microsoft Entra ID Documentation](https://learn.microsoft.com/en-us/entra/identity/)
- [Azure Active Directory (legacy) Documentation](https://docs.microsoft.com/en-us/azure/active-directory/)

## Security Best Practices

1. **Never commit secrets to source control**
2. **Rotate secrets regularly** (every 90 days)
3. **Use managed identities** instead of service principals when possible
4. **Enable soft delete** on Key Vault for production
5. **Monitor Key Vault access logs** for suspicious activity
6. **Use WAF in Prevention mode** for production
7. **Implement least privilege access** for all identities
8. **Enable Azure Security Center** for continuous monitoring
9. **Configure backup and disaster recovery** for Key Vault
10. **Document all security configurations** and keep this guide updated
