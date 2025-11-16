# Deployment Guide

This document provides comprehensive instructions for deploying the Portfolio CMS application to Azure using Docker containers and Azure Container Apps.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Azure Container Registry Setup](#azure-container-registry-setup)
- [GitHub Secrets Configuration](#github-secrets-configuration)
- [Docker Image Build and Push](#docker-image-build-and-push)
- [Infrastructure Deployment](#infrastructure-deployment)
- [Application Deployment](#application-deployment)
- [Monitoring and Observability](#monitoring-and-observability)
- [Troubleshooting](#troubleshooting)

## Overview

The Portfolio CMS uses a containerized deployment strategy with the following components:

- **Docker Containers**: Frontend and API applications packaged as Docker images
- **Azure Container Registry (ACR)**: Private registry for storing Docker images
- **Azure Container Apps**: Managed container hosting with built-in scaling and service discovery
- **Azure SQL Database**: Managed database service (Free tier for development)
- **Azure Key Vault**: Secure secrets management
- **GitHub Actions**: CI/CD pipeline automation

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     GitHub Actions CI/CD                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Build & Test │→ │ Docker Build │→ │   Deploy     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│              Azure Container Registry (ACR)                  │
│  ┌──────────────────────┐  ┌──────────────────────┐        │
│  │ portfoliocms-api     │  │ portfoliocms-frontend│        │
│  │ Image Repository     │  │ Image Repository     │        │
│  └──────────────────────┘  └──────────────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│           Azure Container Apps Environment                   │
│  ┌──────────────────────┐  ┌──────────────────────┐        │
│  │ API Container App    │←→│ Frontend Container   │        │
│  │ (Internal Ingress)   │  │ (External Ingress)   │        │
│  └──────────────────────┘  └──────────────────────┘        │
│              ↓                                               │
│  ┌──────────────────────┐  ┌──────────────────────┐        │
│  │ Azure SQL Database   │  │ Azure Key Vault      │        │
│  └──────────────────────┘  └──────────────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

## Prerequisites

### Required Tools

- **Azure CLI**: Version 2.50.0 or later
- **Docker**: Version 20.10 or later
- **Terraform**: Version 1.6.0 or later
- **.NET SDK**: Version 9.0 or later
- **Git**: Version 2.30 or later

### Azure Subscription

- Active Azure subscription with appropriate permissions
- Resource group creation permissions
- Container Apps and Container Registry permissions
- Key Vault access permissions

### GitHub Repository

- Repository with GitHub Actions enabled
- Secrets management access
- Branch protection rules configured (recommended)

## Azure Container Registry Setup

### 1. Create Azure Container Registry

```bash
# Set variables
RESOURCE_GROUP="rg-portfoliocms-shared"
LOCATION="eastus"
ACR_NAME="acrportfoliocms"  # Must be globally unique

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Azure Container Registry
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true

# Get ACR credentials
az acr credential show --name $ACR_NAME --resource-group $RESOURCE_GROUP
```

### 2. Enable Admin Access

```bash
# Enable admin user (required for GitHub Actions)
az acr update --name $ACR_NAME --admin-enabled true

# Get username and password
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query username -o tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv)

echo "ACR Username: $ACR_USERNAME"
echo "ACR Password: $ACR_PASSWORD"
```

**Important**: Store these credentials securely. You'll need them for GitHub Secrets.

### 3. Configure ACR for Container Apps

```bash
# Allow Container Apps to pull images from ACR
az acr update --name $ACR_NAME --anonymous-pull-enabled false
```

## GitHub Secrets Configuration

Configure the following secrets in your GitHub repository:

### Navigate to Repository Settings

1. Go to your GitHub repository
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**

### Required Secrets

| Secret Name | Description | How to Obtain |
|-------------|-------------|---------------|
| `ACR_USERNAME` | Azure Container Registry username | `az acr credential show --name <acr-name> --query username -o tsv` |
| `ACR_PASSWORD` | Azure Container Registry password | `az acr credential show --name <acr-name> --query passwords[0].value -o tsv` |
| `AZURE_CREDENTIALS` | Azure service principal credentials | See [Service Principal Setup](#service-principal-setup) |
| `ARM_CLIENT_ID` | Azure service principal client ID | From service principal creation |
| `ARM_CLIENT_SECRET` | Azure service principal client secret | From service principal creation |
| `ARM_SUBSCRIPTION_ID` | Azure subscription ID | `az account show --query id -o tsv` |
| `ARM_TENANT_ID` | Azure tenant ID | `az account show --query tenantId -o tsv` |
| `SQL_ADMIN_PASSWORD` | SQL Server admin password | Generate secure password |

### Service Principal Setup

Create a service principal for GitHub Actions:

```bash
# Get subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Create service principal
az ad sp create-for-rbac \
  --name "sp-portfoliocms-github-actions" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth

# Output will be JSON - save this as AZURE_CREDENTIALS secret
```

The output will look like:

```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

Copy the entire JSON output and save it as the `AZURE_CREDENTIALS` secret.

## Docker Image Build and Push

### Local Build and Test

Test Docker builds locally before pushing to CI/CD:

```bash
# Build API image
docker build -f src/PortfolioCMS.API/Dockerfile -t portfoliocms-api:local .

# Build Frontend image
docker build -f src/PortfolioCMS.Frontend/Dockerfile -t portfoliocms-frontend:local .

# Test API container
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development portfoliocms-api:local

# Test Frontend container
docker run -p 8081:8080 -e ASPNETCORE_ENVIRONMENT=Development portfoliocms-frontend:local
```

### Automated Build via GitHub Actions

The `docker-build-push.yml` workflow automatically:

1. Builds Docker images on push to `main` or `develop` branches
2. Runs security scans with Trivy
3. Pushes images to Azure Container Registry
4. Tags images with commit SHA and branch name

**Trigger the workflow**:

```bash
# Push to develop branch
git checkout develop
git push origin develop

# Or manually trigger via GitHub UI
# Actions → Build and Push Docker Images → Run workflow
```

### Manual Push to ACR

If you need to manually push images:

```bash
# Login to ACR
az acr login --name $ACR_NAME

# Tag images
docker tag portfoliocms-api:local $ACR_NAME.azurecr.io/portfoliocms-api:latest
docker tag portfoliocms-frontend:local $ACR_NAME.azurecr.io/portfoliocms-frontend:latest

# Push images
docker push $ACR_NAME.azurecr.io/portfoliocms-api:latest
docker push $ACR_NAME.azurecr.io/portfoliocms-frontend:latest
```

## Infrastructure Deployment

### 1. Initialize Terraform Backend

```bash
cd infrastructure

# Create storage account for Terraform state
STORAGE_ACCOUNT="stportfoliocmstfstate"
CONTAINER_NAME="tfstate"

az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS

az storage container create \
  --name $CONTAINER_NAME \
  --account-name $STORAGE_ACCOUNT
```

### 2. Configure Terraform Variables

Create environment-specific variable files:

```bash
# Create staging variables
cat > environments/staging/terraform.tfvars << EOF
environment = "staging"
location = "eastus"
resource_group_name = "rg-portfoliocms-staging"
sql_server_name = "sql-portfoliocms-staging"
sql_database_name = "sqldb-portfoliocms-staging"
container_registry_name = "acrportfoliocms"
key_vault_name = "kv-portfoliocms-staging"
EOF
```

### 3. Deploy Infrastructure

```bash
# Initialize Terraform
terraform init

# Plan deployment
terraform plan -var-file='environments/staging/terraform.tfvars'

# Apply deployment
terraform apply -var-file='environments/staging/terraform.tfvars' -auto-approve
```

### 4. Configure Key Vault Secrets

After infrastructure deployment, manually set Key Vault secrets:

```bash
KEY_VAULT_NAME="kv-portfoliocms-staging"

# Set SQL admin password
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name SqlAdminPassword \
  --value "YourSecurePassword123!"

# Set SQL connection string
SQL_CONNECTION_STRING="Server=tcp:sql-portfoliocms-staging.database.windows.net,1433;Initial Catalog=sqldb-portfoliocms-staging;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name SqlConnectionString \
  --value "$SQL_CONNECTION_STRING"

# Set Application Insights connection string
APPINSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show \
  --app portfoliocms-appinsights-staging \
  --resource-group rg-portfoliocms-staging \
  --query connectionString -o tsv)

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name AppInsightsConnectionString \
  --value "$APPINSIGHTS_CONNECTION_STRING"
```

## Application Deployment

### Automated Deployment via GitHub Actions

#### Staging Deployment

Automatically triggered on push to `develop` branch:

```bash
git checkout develop
git push origin develop
```

Or manually trigger:

1. Go to **Actions** → **Deploy to Staging**
2. Click **Run workflow**
3. Select branch and options
4. Click **Run workflow**

#### Production Deployment

Requires manual approval:

1. Go to **Actions** → **Deploy to Production**
2. Click **Run workflow**
3. Type `DEPLOY` in the confirmation field
4. Click **Run workflow**

### Manual Deployment to Container Apps

If you need to deploy manually:

```bash
# Set variables
RESOURCE_GROUP="rg-portfoliocms-staging"
CONTAINER_APP_ENV="cae-portfoliocms-staging"
ACR_NAME="acrportfoliocms"
IMAGE_TAG="latest"

# Deploy API Container App
az containerapp create \
  --name ca-portfoliocms-api-staging \
  --resource-group $RESOURCE_GROUP \
  --environment $CONTAINER_APP_ENV \
  --image $ACR_NAME.azurecr.io/portfoliocms-api:$IMAGE_TAG \
  --target-port 8080 \
  --ingress internal \
  --registry-server $ACR_NAME.azurecr.io \
  --registry-username $(az acr credential show --name $ACR_NAME --query username -o tsv) \
  --registry-password $(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv) \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Staging \
    KeyVault__VaultUri=secretref:keyvault-uri \
  --cpu 0.5 \
  --memory 1.0Gi \
  --min-replicas 1 \
  --max-replicas 3

# Deploy Frontend Container App
az containerapp create \
  --name ca-portfoliocms-frontend-staging \
  --resource-group $RESOURCE_GROUP \
  --environment $CONTAINER_APP_ENV \
  --image $ACR_NAME.azurecr.io/portfoliocms-frontend:$IMAGE_TAG \
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
```

### Database Migrations

Run database migrations after deployment:

```bash
# Get SQL connection string from Key Vault
CONNECTION_STRING=$(az keyvault secret show \
  --vault-name kv-portfoliocms-staging \
  --name SqlConnectionString \
  --query value -o tsv)

# Run migrations
dotnet ef database update \
  --project src/PortfolioCMS.DataAccess \
  --startup-project src/PortfolioCMS.API \
  --connection "$CONNECTION_STRING"
```

## Monitoring and Observability

### Application Insights

Access Application Insights dashboards:

```bash
# Get Application Insights URL
az monitor app-insights component show \
  --app portfoliocms-appinsights-staging \
  --resource-group rg-portfoliocms-staging \
  --query appId -o tsv
```

Navigate to: `https://portal.azure.com/#@<tenant>/resource/subscriptions/<subscription>/resourceGroups/<rg>/providers/microsoft.insights/components/<app-insights-name>`

### Container Apps Logs

View real-time logs:

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

### Health Checks

Test application health:

```bash
# Get Frontend URL
FRONTEND_URL=$(az containerapp show \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.configuration.ingress.fqdn -o tsv)

# Test health endpoint
curl https://$FRONTEND_URL/health

# Test API health (internal)
az containerapp exec \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --command "curl http://ca-portfoliocms-api-staging:8080/health"
```

### Metrics and Dashboards

Key metrics to monitor:

- **Request Rate**: Requests per second
- **Response Time**: P50, P95, P99 latencies
- **Error Rate**: 4xx and 5xx responses
- **CPU Usage**: Container CPU utilization
- **Memory Usage**: Container memory utilization
- **Database Connections**: Active connections to SQL Database

## Troubleshooting

### Common Issues

#### 1. Docker Build Failures

**Problem**: Docker build fails with "COPY failed" error

**Solution**:
```bash
# Ensure you're building from repository root
cd /path/to/PortfolioCMS
docker build -f src/PortfolioCMS.API/Dockerfile .
```

#### 2. ACR Authentication Failures

**Problem**: Cannot push images to ACR

**Solution**:
```bash
# Re-login to ACR
az acr login --name $ACR_NAME

# Verify credentials
az acr credential show --name $ACR_NAME
```

#### 3. Container App Deployment Failures

**Problem**: Container app fails to start

**Solution**:
```bash
# Check container logs
az containerapp logs show \
  --name <container-app-name> \
  --resource-group <resource-group> \
  --follow

# Check revision status
az containerapp revision list \
  --name <container-app-name> \
  --resource-group <resource-group>
```

#### 4. Database Connection Failures

**Problem**: Application cannot connect to SQL Database

**Solution**:
```bash
# Verify Key Vault access
az keyvault secret show \
  --vault-name <key-vault-name> \
  --name SqlConnectionString

# Check firewall rules
az sql server firewall-rule list \
  --server <sql-server-name> \
  --resource-group <resource-group>

# Add Container Apps subnet to firewall
az sql server firewall-rule create \
  --server <sql-server-name> \
  --resource-group <resource-group> \
  --name AllowContainerApps \
  --start-ip-address <subnet-start> \
  --end-ip-address <subnet-end>
```

#### 5. Service Discovery Issues

**Problem**: Frontend cannot communicate with API

**Solution**:
```bash
# Verify both apps are in same Container Apps Environment
az containerapp show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.environmentId

az containerapp show \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.environmentId

# Test connectivity from Frontend to API
az containerapp exec \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --command "curl http://ca-portfoliocms-api-staging:8080/health"
```

### Rollback Procedures

#### Rollback to Previous Image

```bash
# List previous revisions
az containerapp revision list \
  --name <container-app-name> \
  --resource-group <resource-group>

# Activate previous revision
az containerapp revision activate \
  --name <container-app-name> \
  --resource-group <resource-group> \
  --revision <previous-revision-name>

# Deactivate current revision
az containerapp revision deactivate \
  --name <container-app-name> \
  --resource-group <resource-group> \
  --revision <current-revision-name>
```

#### Database Rollback

```bash
# Restore from backup
az sql db restore \
  --dest-name sqldb-portfoliocms-staging-restored \
  --edition Free \
  --name sqldb-portfoliocms-staging \
  --resource-group rg-portfoliocms-staging \
  --server sql-portfoliocms-staging \
  --time "2024-01-15T12:00:00Z"
```

### Support and Resources

- **Azure Container Apps Documentation**: https://learn.microsoft.com/azure/container-apps/
- **Azure Container Registry Documentation**: https://learn.microsoft.com/azure/container-registry/
- **.NET Aspire Documentation**: https://learn.microsoft.com/dotnet/aspire/
- **GitHub Actions Documentation**: https://docs.github.com/actions

For additional support, contact the development team or create an issue in the GitHub repository.
