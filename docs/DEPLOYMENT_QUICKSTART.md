# Deployment Quick Start Guide

This guide provides a streamlined path to deploy Portfolio CMS to Azure using Docker containers and Azure Container Apps.

## Prerequisites Checklist

- [ ] Azure subscription with Owner or Contributor role
- [ ] Azure CLI installed and logged in (`az login`)
- [ ] Docker Desktop installed and running
- [ ] Git repository cloned locally
- [ ] GitHub account with repository access

## Step 1: Azure Container Registry Setup (5 minutes)

```bash
# Set your variables
export RESOURCE_GROUP="rg-portfoliocms-shared"
export LOCATION="eastus"
export ACR_NAME="acrportfoliocms$(date +%s)"  # Unique name

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Azure Container Registry
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true

# Get credentials (save these!)
az acr credential show --name $ACR_NAME
```

**Save the output**: You'll need the username and password for GitHub Secrets.

## Step 2: GitHub Secrets Configuration (5 minutes)

Go to your GitHub repository → Settings → Secrets and variables → Actions

Add these secrets:

| Secret Name | Value | Command to Get |
|-------------|-------|----------------|
| `ACR_USERNAME` | ACR username | `az acr credential show --name $ACR_NAME --query username -o tsv` |
| `ACR_PASSWORD` | ACR password | `az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv` |
| `ARM_SUBSCRIPTION_ID` | Subscription ID | `az account show --query id -o tsv` |
| `ARM_TENANT_ID` | Tenant ID | `az account show --query tenantId -o tsv` |
| `SQL_ADMIN_PASSWORD` | Strong password | Generate: `openssl rand -base64 32` |

### Create Service Principal

```bash
# Create service principal for GitHub Actions
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

az ad sp create-for-rbac \
  --name "sp-portfoliocms-github" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth

# Copy the entire JSON output and save as AZURE_CREDENTIALS secret
# Also save clientId as ARM_CLIENT_ID and clientSecret as ARM_CLIENT_SECRET
```

## Step 3: Update Workflow Configuration (2 minutes)

Edit `.github/workflows/docker-build-push.yml`:

```yaml
env:
  REGISTRY_NAME: 'acrportfoliocms1234567890'  # Replace with your ACR name
```

Edit `.github/workflows/deploy-staging.yml`:

```yaml
env:
  REGISTRY_NAME: 'acrportfoliocms1234567890'  # Replace with your ACR name
```

Commit and push changes:

```bash
git add .github/workflows/
git commit -m "Configure ACR name for deployment"
git push origin main
```

## Step 4: Build and Push Docker Images (10 minutes)

### Option A: Automated via GitHub Actions (Recommended)

```bash
# Push to main branch to trigger build
git checkout main
git push origin main

# Or manually trigger workflow
# Go to Actions → Build and Push Docker Images → Run workflow
```

### Option B: Manual Build and Push

```bash
# Login to ACR
az acr login --name $ACR_NAME

# Build and push API
docker build -f src/PortfolioCMS.API/Dockerfile -t $ACR_NAME.azurecr.io/portfoliocms-api:latest .
docker push $ACR_NAME.azurecr.io/portfoliocms-api:latest

# Build and push Frontend
docker build -f src/PortfolioCMS.Frontend/Dockerfile -t $ACR_NAME.azurecr.io/portfoliocms-frontend:latest .
docker push $ACR_NAME.azurecr.io/portfoliocms-frontend:latest
```

## Step 5: Deploy Infrastructure with Terraform (15 minutes)

```bash
cd infrastructure

# Initialize Terraform
terraform init

# Create staging environment variables
cat > environments/staging/terraform.tfvars << EOF
environment = "staging"
location = "eastus"
resource_group_name = "rg-portfoliocms-staging"
sql_server_name = "sql-portfoliocms-staging-$(date +%s)"
sql_database_name = "sqldb-portfoliocms"
container_registry_name = "$ACR_NAME"
key_vault_name = "kv-portfoliocms-$(date +%s)"
sql_admin_password = "$(openssl rand -base64 32)"
EOF

# Plan deployment
terraform plan -var-file='environments/staging/terraform.tfvars'

# Apply deployment
terraform apply -var-file='environments/staging/terraform.tfvars' -auto-approve

cd ..
```

## Step 6: Configure Key Vault Secrets (5 minutes)

```bash
# Get Key Vault name from Terraform output
KEY_VAULT_NAME=$(cd infrastructure && terraform output -raw key_vault_name)

# Set SQL connection string
SQL_SERVER=$(cd infrastructure && terraform output -raw sql_server_fqdn)
SQL_DATABASE=$(cd infrastructure && terraform output -raw sql_database_name)
SQL_PASSWORD=$(cd infrastructure && terraform output -raw sql_admin_password)

CONNECTION_STRING="Server=tcp:$SQL_SERVER,1433;Initial Catalog=$SQL_DATABASE;Persist Security Info=False;User ID=sqladmin;Password=$SQL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name SqlConnectionString \
  --value "$CONNECTION_STRING"

# Set Application Insights connection string
APPINSIGHTS_NAME=$(cd infrastructure && terraform output -raw application_insights_name)
APPINSIGHTS_CONNECTION=$(az monitor app-insights component show \
  --app $APPINSIGHTS_NAME \
  --resource-group rg-portfoliocms-staging \
  --query connectionString -o tsv)

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name AppInsightsConnectionString \
  --value "$APPINSIGHTS_CONNECTION"

echo "✅ Key Vault secrets configured"
```

## Step 7: Deploy Applications to Container Apps (10 minutes)

### Option A: Automated via GitHub Actions (Recommended)

```bash
# Push to develop branch to trigger staging deployment
git checkout -b develop
git push origin develop

# Monitor deployment in GitHub Actions
# Go to Actions → Deploy to Staging
```

### Option B: Manual Deployment

```bash
# Get infrastructure details
RESOURCE_GROUP="rg-portfoliocms-staging"
CONTAINER_APP_ENV=$(cd infrastructure && terraform output -raw container_apps_environment_name)

# Deploy API Container App
az containerapp create \
  --name ca-portfoliocms-api-staging \
  --resource-group $RESOURCE_GROUP \
  --environment $CONTAINER_APP_ENV \
  --image $ACR_NAME.azurecr.io/portfoliocms-api:latest \
  --target-port 8080 \
  --ingress internal \
  --registry-server $ACR_NAME.azurecr.io \
  --registry-username $(az acr credential show --name $ACR_NAME --query username -o tsv) \
  --registry-password $(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv) \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Staging \
    KeyVault__VaultUri=https://$KEY_VAULT_NAME.vault.azure.net/ \
  --cpu 0.5 \
  --memory 1.0Gi \
  --min-replicas 1 \
  --max-replicas 3

# Deploy Frontend Container App
az containerapp create \
  --name ca-portfoliocms-frontend-staging \
  --resource-group $RESOURCE_GROUP \
  --environment $CONTAINER_APP_ENV \
  --image $ACR_NAME.azurecr.io/portfoliocms-frontend:latest \
  --target-port 8080 \
  --ingress external \
  --registry-server $ACR_NAME.azurecr.io \
  --registry-username $(az acr credential show --name $ACR_NAME --query username -o tsv) \
  --registry-password $(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv) \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Staging \
    services__api__http__0=http://ca-portfoliocms-api-staging \
  --cpu 0.5 \
  --memory 1.0Gi \
  --min-replicas 1 \
  --max-replicas 5

echo "✅ Container Apps deployed"
```

## Step 8: Run Database Migrations (5 minutes)

```bash
# Get connection string from Key Vault
CONNECTION_STRING=$(az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name SqlConnectionString \
  --query value -o tsv)

# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Run migrations
dotnet ef database update \
  --project src/PortfolioCMS.DataAccess \
  --startup-project src/PortfolioCMS.API \
  --connection "$CONNECTION_STRING"

echo "✅ Database migrations completed"
```

## Step 9: Verify Deployment (5 minutes)

```bash
# Get Frontend URL
FRONTEND_URL=$(az containerapp show \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.configuration.ingress.fqdn -o tsv)

echo "Frontend URL: https://$FRONTEND_URL"

# Test health endpoint
curl https://$FRONTEND_URL/health

# Open in browser
open https://$FRONTEND_URL  # macOS
start https://$FRONTEND_URL  # Windows
xdg-open https://$FRONTEND_URL  # Linux
```

## Step 10: Monitor Application (Ongoing)

### View Logs

```bash
# API logs
az containerapp logs show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --follow

# Frontend logs
az containerapp logs show \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --follow
```

### Access Application Insights

```bash
# Get Application Insights URL
APPINSIGHTS_NAME=$(cd infrastructure && terraform output -raw application_insights_name)
echo "Application Insights: https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/rg-portfoliocms-staging/providers/microsoft.insights/components/$APPINSIGHTS_NAME"
```

### View Metrics

```bash
# Container App metrics
az containerapp show \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.configuration.ingress
```

## Troubleshooting

### Container App Not Starting

```bash
# Check revision status
az containerapp revision list \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging

# View detailed logs
az containerapp logs show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --tail 100
```

### Database Connection Issues

```bash
# Verify Key Vault access
az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name SqlConnectionString

# Check SQL Server firewall
az sql server firewall-rule list \
  --server $(cd infrastructure && terraform output -raw sql_server_name) \
  --resource-group rg-portfoliocms-staging
```

### Image Pull Failures

```bash
# Verify ACR credentials
az acr credential show --name $ACR_NAME

# Test image pull
docker pull $ACR_NAME.azurecr.io/portfoliocms-api:latest
```

## Next Steps

1. **Configure Custom Domain**: Add custom domain to Container App
2. **Set Up SSL Certificate**: Configure SSL/TLS certificate
3. **Configure Monitoring Alerts**: Set up alerts in Application Insights
4. **Enable Auto-scaling**: Configure scaling rules based on metrics
5. **Deploy to Production**: Follow same steps for production environment

## Cost Estimation

**Staging Environment (Monthly)**:
- Azure Container Apps: ~$30-50
- Azure SQL Database (Free tier): $0
- Azure Container Registry (Basic): ~$5
- Application Insights: ~$10-20
- Key Vault: ~$1
- **Total**: ~$46-76/month

**Production Environment (Monthly)**:
- Azure Container Apps: ~$100-200
- Azure SQL Database (Basic): ~$5
- Azure Container Registry (Basic): ~$5
- Application Insights: ~$50-100
- Key Vault: ~$1
- Azure CDN: ~$10-20
- **Total**: ~$171-331/month

## Support

For detailed documentation:
- [Full Deployment Guide](./DEPLOYMENT.md)
- [Docker Guide](./DOCKER.md)
- [Infrastructure Documentation](../infrastructure/README.md)

For issues, create a GitHub issue or contact the development team.
