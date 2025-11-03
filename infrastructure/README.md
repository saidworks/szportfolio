# Portfolio CMS Infrastructure

This directory contains the Infrastructure as Code (IaC) configuration for the Portfolio CMS application using Terraform/OpenTofu.

## Architecture Overview

The infrastructure deploys a three-tier architecture on Azure:

- **Frontend**: Blazor Server application hosted on Azure App Service
- **API**: ASP.NET Core Web API hosted on Azure App Service  
- **Data**: Azure SQL Database with Entity Framework Core

## Prerequisites

1. **Azure CLI** - Install and configure Azure CLI
2. **Terraform/OpenTofu** - Install Terraform or OpenTofu
3. **Azure Subscription** - Active Azure subscription with appropriate permissions

## Quick Start

### 1. Configure Azure Authentication

```bash
# Login to Azure
az login

# Set the subscription (if you have multiple)
az account set --subscription "your-subscription-id"
```

### 2. Initialize Terraform Backend

Before running Terraform, you need to create the storage account for state management:

```bash
# Create resource group for Terraform state
az group create --name rg-portfoliocms-tfstate --location "East US"

# Create storage account for Terraform state
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

### 3. Deploy Infrastructure

```bash
# Navigate to infrastructure directory
cd infrastructure

# Initialize Terraform
terraform init

# Plan deployment (development environment)
terraform plan -var-file="environments/development/terraform.tfvars"

# Apply deployment
terraform apply -var-file="environments/development/terraform.tfvars"
```

## Environment Management

The infrastructure supports three environments:

- **Development** (`environments/development/`)
- **Staging** (`environments/staging/`)
- **Production** (`environments/production/`)

### Deploy to Specific Environment

```bash
# Development
terraform plan -var-file="environments/development/terraform.tfvars"
terraform apply -var-file="environments/development/terraform.tfvars"

# Staging
terraform plan -var-file="environments/staging/terraform.tfvars"
terraform apply -var-file="environments/staging/terraform.tfvars"

# Production
terraform plan -var-file="environments/production/terraform.tfvars"
terraform apply -var-file="environments/production/terraform.tfvars"
```

## Configuration

### Required Variables

The following variables must be configured in your `terraform.tfvars` file:

```hcl
# SQL Database password (sensitive)
sql_admin_password = "YourSecurePassword123!"
```

### Environment Variables

For CI/CD pipelines, set these environment variables:

```bash
export TF_VAR_sql_admin_password="YourSecurePassword123!"
export ARM_CLIENT_ID="your-service-principal-client-id"
export ARM_CLIENT_SECRET="your-service-principal-client-secret"
export ARM_SUBSCRIPTION_ID="your-subscription-id"
export ARM_TENANT_ID="your-tenant-id"
```

## Resources Created

The Terraform configuration creates the following Azure resources:

### Core Infrastructure
- **Resource Group** - Container for all resources
- **App Service Plan** - Hosting plan for web applications
- **App Services** (2) - Frontend and API applications
- **SQL Server** - Database server
- **SQL Database** - Application database

### Storage & Security
- **Storage Account** - Media file storage
- **Storage Container** - Blob container for media files
- **Key Vault** - Secrets management

### Monitoring & Observability
- **Log Analytics Workspace** - Centralized logging
- **Application Insights** - Application performance monitoring

## Outputs

After deployment, Terraform provides the following outputs:

- `frontend_app_url` - URL of the frontend application
- `api_app_url` - URL of the API application
- `sql_server_fqdn` - SQL Server fully qualified domain name
- `key_vault_uri` - Key Vault URI for application configuration
- `application_insights_connection_string` - Application Insights connection string

## Security Considerations

1. **Secrets Management**: All sensitive values are stored in Azure Key Vault
2. **Network Security**: SQL Server firewall rules restrict access to Azure services only
3. **HTTPS Only**: All App Services enforce HTTPS
4. **Managed Identity**: Applications use managed identity for Azure service authentication

## Cost Optimization

### Development Environment
- Basic App Service Plan (B1)
- Basic SQL Database
- Standard LRS storage replication

### Production Environment
- Premium App Service Plan (P1v2) with auto-scaling
- Standard SQL Database with geo-replication
- GRS storage replication for high availability

## Monitoring and Alerting

The infrastructure includes comprehensive monitoring:

- **Application Insights** for application telemetry
- **Log Analytics** for centralized logging
- **Azure Monitor** for infrastructure metrics
- **Grafana Cloud** integration (configured in application)

## Backup and Disaster Recovery

- **SQL Database**: Automated backups with 35-day retention
- **Storage Account**: Geo-redundant storage in production
- **Infrastructure**: Version-controlled Terraform state

## Troubleshooting

### Common Issues

1. **Storage Account Name Conflicts**
   - Storage account names must be globally unique
   - Modify `storage_account_name` variable if deployment fails

2. **SQL Server Name Conflicts**
   - SQL server names must be globally unique
   - Modify `sql_server_name` variable if deployment fails

3. **Permission Issues**
   - Ensure your Azure account has Contributor role on the subscription
   - For CI/CD, ensure service principal has appropriate permissions

### Useful Commands

```bash
# Check Terraform state
terraform show

# List all resources
terraform state list

# Get specific resource information
terraform state show azurerm_linux_web_app.frontend

# Destroy infrastructure (be careful!)
terraform destroy -var-file="environments/development/terraform.tfvars"
```

## CI/CD Integration

This infrastructure is designed to work with GitHub Actions. See `.github/workflows/` for deployment pipelines.

The CI/CD pipeline:
1. Validates Terraform configuration
2. Plans infrastructure changes
3. Applies changes to staging environment
4. Runs tests against staging
5. Deploys to production (manual approval required)

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review Azure resource logs in the Azure Portal
3. Check Terraform state and plan output
4. Consult Azure documentation for specific resource issues