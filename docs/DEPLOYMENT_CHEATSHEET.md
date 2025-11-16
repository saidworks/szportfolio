# Deployment Cheat Sheet

Quick reference for common deployment tasks.

## 🐳 Docker Commands

### Build Images
```bash
# API
docker build -f src/PortfolioCMS.API/Dockerfile -t portfoliocms-api:latest .

# Frontend
docker build -f src/PortfolioCMS.Frontend/Dockerfile -t portfoliocms-frontend:latest .
```

### Run Containers
```bash
# API (port 8080)
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development portfoliocms-api:latest

# Frontend (port 8081)
docker run -p 8081:8080 -e ASPNETCORE_ENVIRONMENT=Development portfoliocms-frontend:latest

# Docker Compose (all services)
docker-compose up -d
```

### Manage Containers
```bash
# View logs
docker logs -f portfoliocms-api
docker-compose logs -f

# Stop containers
docker stop portfoliocms-api portfoliocms-frontend
docker-compose down

# Remove containers and volumes
docker-compose down -v
```

## ☁️ Azure Container Registry

### Login and Push
```bash
# Login
az acr login --name acrportfoliocms

# Tag images
docker tag portfoliocms-api:latest acrportfoliocms.azurecr.io/portfoliocms-api:latest
docker tag portfoliocms-frontend:latest acrportfoliocms.azurecr.io/portfoliocms-frontend:latest

# Push images
docker push acrportfoliocms.azurecr.io/portfoliocms-api:latest
docker push acrportfoliocms.azurecr.io/portfoliocms-frontend:latest
```

### List Images
```bash
# List repositories
az acr repository list --name acrportfoliocms

# List tags
az acr repository show-tags --name acrportfoliocms --repository portfoliocms-api
```

## 🚀 Azure Container Apps

### Deploy Container App
```bash
# API
az containerapp create \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --environment cae-portfoliocms-staging \
  --image acrportfoliocms.azurecr.io/portfoliocms-api:latest \
  --target-port 8080 \
  --ingress internal \
  --cpu 0.5 --memory 1.0Gi \
  --min-replicas 1 --max-replicas 3

# Frontend
az containerapp create \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --environment cae-portfoliocms-staging \
  --image acrportfoliocms.azurecr.io/portfoliocms-frontend:latest \
  --target-port 8080 \
  --ingress external \
  --cpu 0.5 --memory 1.0Gi \
  --min-replicas 1 --max-replicas 5
```

### Update Container App
```bash
# Update with new image
az containerapp update \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --image acrportfoliocms.azurecr.io/portfoliocms-api:new-tag
```

### View Logs
```bash
# Stream logs
az containerapp logs show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --follow

# Tail logs
az containerapp logs show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --tail 100
```

### Get Container App URL
```bash
FRONTEND_URL=$(az containerapp show \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.configuration.ingress.fqdn -o tsv)
echo "https://$FRONTEND_URL"
```

## 🔑 Azure Key Vault

### Set Secrets
```bash
# SQL connection string
az keyvault secret set \
  --vault-name kv-portfoliocms-staging \
  --name SqlConnectionString \
  --value "Server=tcp:..."

# Application Insights
az keyvault secret set \
  --vault-name kv-portfoliocms-staging \
  --name AppInsightsConnectionString \
  --value "InstrumentationKey=..."
```

### Get Secrets
```bash
# Show secret value
az keyvault secret show \
  --vault-name kv-portfoliocms-staging \
  --name SqlConnectionString \
  --query value -o tsv

# List all secrets
az keyvault secret list \
  --vault-name kv-portfoliocms-staging
```

## 🗄️ Database Migrations

### Run Migrations
```bash
# Get connection string from Key Vault
CONNECTION_STRING=$(az keyvault secret show \
  --vault-name kv-portfoliocms-staging \
  --name SqlConnectionString \
  --query value -o tsv)

# Apply migrations
dotnet ef database update \
  --project src/PortfolioCMS.DataAccess \
  --startup-project src/PortfolioCMS.API \
  --connection "$CONNECTION_STRING"
```

### Create Migration
```bash
# Add new migration
dotnet ef migrations add MigrationName \
  --project src/PortfolioCMS.DataAccess \
  --startup-project src/PortfolioCMS.API

# Remove last migration
dotnet ef migrations remove \
  --project src/PortfolioCMS.DataAccess \
  --startup-project src/PortfolioCMS.API
```

## 🏗️ Terraform

### Initialize and Deploy
```bash
cd infrastructure

# Initialize
terraform init

# Plan
terraform plan -var-file='environments/staging/terraform.tfvars'

# Apply
terraform apply -var-file='environments/staging/terraform.tfvars' -auto-approve

# Destroy
terraform destroy -var-file='environments/staging/terraform.tfvars'
```

### View Outputs
```bash
# All outputs
terraform output

# Specific output
terraform output -raw sql_server_fqdn
terraform output -raw key_vault_name
```

## 🔍 Monitoring

### Application Insights
```bash
# Get connection string
az monitor app-insights component show \
  --app portfoliocms-appinsights-staging \
  --resource-group rg-portfoliocms-staging \
  --query connectionString -o tsv

# Query logs
az monitor app-insights query \
  --app portfoliocms-appinsights-staging \
  --resource-group rg-portfoliocms-staging \
  --analytics-query "requests | where timestamp > ago(1h) | summarize count() by resultCode"
```

### Container Metrics
```bash
# Get metrics
az monitor metrics list \
  --resource /subscriptions/.../resourceGroups/rg-portfoliocms-staging/providers/Microsoft.App/containerApps/ca-portfoliocms-api-staging \
  --metric "Requests"
```

## 🔄 GitHub Actions

### Trigger Workflows
```bash
# Push to trigger build
git push origin main

# Manual trigger (via GitHub CLI)
gh workflow run docker-build-push.yml

# View workflow runs
gh run list --workflow=docker-build-push.yml
```

### View Logs
```bash
# List recent runs
gh run list

# View run logs
gh run view <run-id> --log
```

## 🧪 Health Checks

### Local
```bash
# API health
curl http://localhost:8080/health

# Frontend health
curl http://localhost:8081/health
```

### Azure
```bash
# Get URL and test
FRONTEND_URL=$(az containerapp show \
  --name ca-portfoliocms-frontend-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.configuration.ingress.fqdn -o tsv)

curl https://$FRONTEND_URL/health
```

## 🔧 Troubleshooting

### View Container Logs
```bash
# Azure Container Apps
az containerapp logs show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --follow

# Docker
docker logs -f portfoliocms-api
```

### Restart Container App
```bash
az containerapp revision restart \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging
```

### Check Container Status
```bash
# List revisions
az containerapp revision list \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging

# Show container app details
az containerapp show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging
```

### Database Connection Test
```bash
# Test from container
az containerapp exec \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --command "/bin/bash"
```

## 📊 Common Queries

### Application Insights Queries
```kusto
// Request rate
requests
| where timestamp > ago(1h)
| summarize count() by bin(timestamp, 5m)

// Error rate
requests
| where timestamp > ago(1h)
| summarize ErrorRate = countif(success == false) * 100.0 / count() by bin(timestamp, 5m)

// Slowest requests
requests
| where timestamp > ago(1h)
| top 10 by duration desc
| project timestamp, name, duration, resultCode

// Exceptions
exceptions
| where timestamp > ago(1h)
| summarize count() by type, outerMessage
```

## 🔐 Security

### Scan Docker Image
```bash
# Trivy scan
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:latest image portfoliocms-api:latest

# Scan with severity filter
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:latest image --severity HIGH,CRITICAL portfoliocms-api:latest
```

### Rotate Secrets
```bash
# Generate new password
NEW_PASSWORD=$(openssl rand -base64 32)

# Update Key Vault
az keyvault secret set \
  --vault-name kv-portfoliocms-staging \
  --name SqlAdminPassword \
  --value "$NEW_PASSWORD"

# Restart apps to pick up new secret
az containerapp restart \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging
```

## 📚 Quick Links

- **Azure Portal**: https://portal.azure.com
- **GitHub Actions**: https://github.com/your-repo/actions
- **Docker Hub**: https://hub.docker.com
- **Terraform Registry**: https://registry.terraform.io

## 💡 Tips

1. **Always test locally first**: Use `docker-compose up` before deploying
2. **Use tags for versioning**: Tag images with version numbers, not just `latest`
3. **Monitor costs**: Check Azure Cost Management regularly
4. **Set up alerts**: Configure alerts for critical metrics
5. **Backup regularly**: Ensure database backups are configured
6. **Use staging**: Always deploy to staging before production
7. **Document changes**: Update documentation when making infrastructure changes
8. **Review logs**: Check logs after every deployment
