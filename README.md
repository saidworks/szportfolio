# Portfolio CMS

> A modern, full-stack content management system for personal portfolios, built with Blazor, ASP.NET Core, and MySQL.

[![.NET](https://img.shields.io/badge/.NET-6.0+-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![MySQL](https://img.shields.io/badge/MySQL-8.0+-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Azure](https://img.shields.io/badge/Azure-Hosted-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**🤖 Built with [Kiro AI] - Agentic IDE**  
This project is a playground for experimenting with spec driven development using Kiro AI, an AI-powered development environment created by AWS. 


## 🎯 Overview

Portfolio CMS is a content management system designed for developers to showcase their work, publish technical articles, and engage with visitors. Built with modern .NET technologies and following clean architecture principles, it demonstrates enterprise-level development practices while remaining easy to deploy and maintain.

### Key Highlights

- **Three-Project Architecture:** Cleanly separated Frontend (Blazor), Data Access Layer, and API Backend
- **Production-Ready:** Hosted on Azure with full CI/CD pipeline via GitHub Actions
- **Infrastructure as Code:** Complete Azure infrastructure defined in OpenTofu/Terraform
- **Comprehensive Observability:** Integrated with Grafana Cloud (free tier) and Azure Application Insights
- **Security-First:** SonarCloud static analysis, OWASP ZAP scanning, vulnerability management
- **API-First Design:** RESTful API with complete Swagger/OpenAPI documentation

---

## 🏗️ Architecture

### Multi-Project Solution Structure

The application is organized into **three independent, deployable projects**:

```
src/
├── PortfolioCMS.API/          # RESTful API Backend (.NET 9)
├── PortfolioCMS.DataAccess/   # Entity Framework Data Layer
├── PortfolioCMS.Frontend/     # Blazor WebAssembly Frontend
└── Tests/                     # Unit & Integration Tests
```

**🔗 Project Dependencies:**
- **Frontend** → API (HTTP calls)
- **API** → DataAccess (Entity Framework)
- **DataAccess** → SQL Server Database

---

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed on your development machine:

#### Required Software

1. **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** (Latest LTS)
   ```bash
   # Verify installation
   dotnet --version
   # Should show 9.0.x or higher
   ```

2. **[SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)** (Included with Visual Studio)
   ```bash
   # Verify installation (Windows)
   sqllocaldb info
   # Should list available LocalDB instances
   ```

3. **[Git](https://git-scm.com/downloads)** for version control
   ```bash
   git --version
   ```

#### Recommended Development Tools

- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** (Community/Professional/Enterprise)
  - Workloads: ASP.NET and web development, .NET desktop development
- **[Visual Studio Code](https://code.visualstudio.com/)** with C# extension
- **[Postman](https://www.postman.com/)** or **[Insomnia](https://insomnia.rest/)** for API testing
- **[SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/)** for database management

### 🛠️ Development Setup

#### 1. Clone the Repository

```bash
git clone https://github.com/your-username/portfolio-cms.git
cd portfolio-cms
```

#### 2. Database Setup

The application uses SQL Server LocalDB for development. The database will be created automatically on first run.

```bash
# Navigate to the project root
cd portfolio-cms

# Restore packages
dotnet restore

# Create and seed the database
dotnet ef database update --project src/PortfolioCMS.DataAccess --startup-project src/PortfolioCMS.API

# Verify database creation
dotnet ef database list --project src/PortfolioCMS.DataAccess --startup-project src/PortfolioCMS.API
```

**Default Admin Account:**
- **Email:** `admin@portfoliocms.dev`
- **Password:** `DevAdmin123!`

#### 3. Configuration

The application uses different configuration files for different environments:

**Development Configuration:**
- `src/PortfolioCMS.API/appsettings.Development.json` - API settings
- Connection strings and JWT secrets are pre-configured for local development

**Optional: Application Insights (for telemetry)**
```json
// In appsettings.Development.json
{
  "ConnectionStrings": {
    "ApplicationInsights": "your-application-insights-connection-string"
  }
}
```

#### 4. Build the Solution

```bash
# Restore NuGet packages and build all projects
dotnet restore
dotnet build

# Or build specific projects
dotnet build src/PortfolioCMS.API
dotnet build src/PortfolioCMS.Frontend
```

#### 5. Run the Application

**Option A: Run API and Frontend Separately (Recommended for Development)**

```bash
# Terminal 1: Start the API Backend
cd src/PortfolioCMS.API
dotnet run
# API will be available at: http://localhost:5000

# Terminal 2: Start the Frontend (in a new terminal)
cd src/PortfolioCMS.Frontend  
dotnet run
# Frontend will be available at: http://localhost:5001
```

**Option B: Run with Visual Studio**
1. Open `PortfolioCMS.sln` in Visual Studio
2. Set multiple startup projects:
   - Right-click solution → Properties → Multiple startup projects
   - Set both `PortfolioCMS.API` and `PortfolioCMS.Frontend` to "Start"
3. Press F5 or click "Start"

#### 6. Verify Installation

**API Health Check:**
```bash
curl http://localhost:5000/health
# Should return: {"status":"Healthy",...}
```

**API Documentation:**
- Open http://localhost:5000/swagger in your browser
- You should see the complete API documentation

**Frontend:**
- Open http://localhost:5001 in your browser
- You should see the Portfolio CMS interface

### 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/Tests/PortfolioCMS.Tests.csproj
```

### 📊 Development Workflow

#### Daily Development

1. **Start Development Environment:**
   ```bash
   # Start API
   cd src/PortfolioCMS.API && dotnet run &
   
   # Start Frontend  
   cd src/PortfolioCMS.Frontend && dotnet run &
   ```

2. **Make Changes:**
   - API changes: Modify controllers, services, or data models
   - Frontend changes: Update Blazor components and pages
   - Database changes: Add migrations with `dotnet ef migrations add`

3. **Test Changes:**
   ```bash
   # Run tests
   dotnet test
   
   # Test API endpoints
   curl http://localhost:5000/api/v1/articles
   ```

4. **Database Migrations:**
   ```bash
   # Add new migration
   dotnet ef migrations add MigrationName --project src/PortfolioCMS.DataAccess --startup-project src/PortfolioCMS.API
   
   # Apply migration
   dotnet ef database update --project src/PortfolioCMS.DataAccess --startup-project src/PortfolioCMS.API
   ```

#### Code Style and Quality

```bash
# Format code
dotnet format

# Analyze code quality (if configured)
dotnet sonarscanner begin /k:"portfolio-cms"
dotnet build
dotnet sonarscanner end
```

### 🔧 Troubleshooting

#### Common Issues

**Port Already in Use:**
```bash
# Windows: Check what's using the port
netstat -ano | findstr :5000

# Kill the process (replace PID)
taskkill /PID <process-id> /F

# macOS/Linux: Check and kill process
lsof -ti:5000 | xargs kill -9
```

**Database Connection Issues:**
```bash
# Verify LocalDB is running (Windows)
sqllocaldb start MSSQLLocalDB

# Reset database
dotnet ef database drop --project src/PortfolioCMS.DataAccess --startup-project src/PortfolioCMS.API
dotnet ef database update --project src/PortfolioCMS.DataAccess --startup-project src/PortfolioCMS.API
```

**Build Errors:**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

**Missing Entity Framework Tools:**
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Update EF Core tools
dotnet tool update --global dotnet-ef
```

### 📚 Additional Resources

- **[.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)**
- **[Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)**
- **[Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)**
- **[ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)**

### 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes following the coding standards
4. Run tests (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

---

## ☁️ Deployment

### Deployment Options

The Portfolio CMS supports multiple deployment strategies:

1. **🐳 Docker Containers** - Containerized deployment with Docker and Azure Container Apps (Recommended)
2. **☁️ Azure App Services** - Traditional PaaS deployment
3. **🏠 Local Development** - Docker Compose for local testing

### Quick Start: Docker Deployment

**Prerequisites:**
- Docker Desktop installed
- Azure CLI installed
- Azure subscription with appropriate permissions

**5-Minute Deployment:**

```bash
# 1. Build Docker images
docker build -f src/PortfolioCMS.API/Dockerfile -t portfoliocms-api:latest .
docker build -f src/PortfolioCMS.Frontend/Dockerfile -t portfoliocms-frontend:latest .

# 2. Run with Docker Compose
docker-compose up -d

# 3. Access the application
# Frontend: http://localhost:8081
# API: http://localhost:8080
# Swagger: http://localhost:8080/swagger
```

**📖 Detailed Guides:**
- **[Quick Start Guide](docs/DEPLOYMENT_QUICKSTART.md)** - 10-minute Azure deployment
- **[Full Deployment Guide](docs/DEPLOYMENT.md)** - Comprehensive deployment documentation
- **[Docker Guide](docs/DOCKER.md)** - Docker containerization details

### CI/CD Pipeline

The project includes comprehensive automated CI/CD pipelines with GitHub Actions, covering build, test, security scanning, and deployment automation. All pipelines are production-ready and follow industry best practices.

#### Pipeline Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    GitHub Actions CI/CD                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Push/PR → CI Pipeline → Security Scans → Docker Build      │
│              ↓              ↓                ↓               │
│           Build/Test    SonarCloud      Push to ACR         │
│           Coverage      OWASP ZAP                            │
│           CodeQL        Snyk                                 │
│           Trivy         Dependency Check                     │
│                                                               │
│  develop branch → Auto Deploy to Staging                     │
│  main branch → Manual Approval → Deploy to Production       │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

#### Available Workflows

| Workflow | Trigger | Purpose | Status |
|----------|---------|---------|--------|
| **CI Pipeline** | Push/PR to `main`/`develop` | Build, test, and validate code quality | ✅ Active |
| **Docker Build & Push** | Push to `main`/`develop` | Build and push Docker images to ACR | ✅ Active |
| **SonarCloud Analysis** | Push/PR to `main`/`develop` | Static code analysis and quality gates | ✅ Active |
| **Security Scanning** | Daily schedule / Manual | Comprehensive security vulnerability scanning | ✅ Active |
| **OWASP ZAP Scan** | Weekly / Manual | Dynamic application security testing | ✅ Active |
| **Deploy to Staging** | Push to `develop` | Automated staging deployment | ✅ Active |
| **Deploy to Production** | Manual approval | Production deployment with approval | ✅ Active |
| **Infrastructure Deploy** | Manual | Terraform infrastructure deployment | ✅ Active |

**Workflow Files:**
- `.github/workflows/ci.yml` - Continuous integration pipeline
- `.github/workflows/docker-build-push.yml` - Docker image build and push
- `.github/workflows/sonarcloud.yml` - SonarCloud static analysis
- `.github/workflows/security-scan.yml` - Comprehensive security scanning
- `.github/workflows/security-scan-zap.yml` - OWASP ZAP security testing
- `.github/workflows/deploy-staging.yml` - Staging deployment automation
- `.github/workflows/deploy-production.yml` - Production deployment
- `.github/workflows/infrastructure-deploy.yml` - Infrastructure as code deployment

#### Detailed Workflow Documentation

##### 1. CI Pipeline (`.github/workflows/ci.yml`)

**Purpose:** Validates code quality, runs tests, and ensures build integrity on every push and pull request.

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests targeting `main` or `develop`
- Manual workflow dispatch

**Jobs:**
1. **Build and Test**
   - Restores NuGet packages with caching
   - Builds solution in Release configuration
   - Runs unit tests with code coverage
   - Uploads test results and coverage reports

2. **Code Quality Analysis**
   - Runs CodeQL security analysis
   - Checks for code smells and technical debt
   - Validates coding standards

**Artifacts Generated:**
- Test results (TRX format)
- Code coverage reports
- Build logs

**Typical Duration:** 5-8 minutes

##### 2. Docker Build & Push (`.github/workflows/docker-build-push.yml`)

**Purpose:** Builds Docker images for API and Frontend, scans for vulnerabilities, and pushes to Azure Container Registry.

**Triggers:**
- Push to `main` or `develop` branches (after successful CI)
- Manual workflow dispatch with environment selection

**Jobs:**
1. **Build and Test** - Validates solution before containerization
2. **Build API Image** - Creates and pushes API Docker image
3. **Build Frontend Image** - Creates and pushes Frontend Docker image
4. **Create Deployment Manifest** - Generates deployment metadata

**Key Features:**
- Multi-stage Docker builds for minimal image size
- BuildKit caching for faster builds
- Trivy vulnerability scanning
- SARIF results uploaded to GitHub Security tab
- Image tagging with commit SHA and branch name

**Image Tags:**
- `{commit-sha}` - Specific commit version
- `latest` - Most recent build
- `{branch}-{commit-sha}` - Branch-specific version

**Artifacts Generated:**
- Docker images in ACR
- Trivy security scan results (SARIF)
- Deployment manifest (JSON)

**Typical Duration:** 10-15 minutes

##### 3. SonarCloud Analysis (`.github/workflows/sonarcloud.yml`)

**Purpose:** Performs static code analysis to identify bugs, vulnerabilities, code smells, and technical debt.

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests targeting `main`

**Analysis Includes:**
- Code quality metrics
- Security vulnerability detection
- Code coverage analysis
- Duplicated code detection
- Maintainability ratings

**Quality Gates:**
- Code Coverage: > 80%
- Duplicated Lines: < 3%
- Maintainability Rating: A
- Reliability Rating: A
- Security Rating: A
- Security Hotspots: Reviewed

**Exclusions:**
- Migration files
- DTOs and entity models
- Program.cs and Startup.cs
- Generated code

**Typical Duration:** 8-12 minutes

##### 4. Security Scanning (`.github/workflows/security-scan.yml`)

**Purpose:** Comprehensive security vulnerability scanning across dependencies, code, and containers.

**Triggers:**
- Daily at 2 AM UTC
- Manual workflow dispatch

**Scans Performed:**
1. **Dependency Scanning** (Snyk)
   - NuGet package vulnerabilities
   - Known CVEs in dependencies
   - License compliance

2. **OWASP Dependency Check**
   - Known vulnerability database
   - CVE identification
   - CVSS scoring

3. **CodeQL Analysis**
   - Security-focused static analysis
   - SQL injection detection
   - XSS vulnerability detection
   - Authentication/authorization issues

4. **Container Scanning** (Trivy)
   - Base image vulnerabilities
   - OS package vulnerabilities
   - Application dependency vulnerabilities

**Artifacts Generated:**
- Security scan reports (HTML, JSON, SARIF)
- Vulnerability summaries
- Remediation recommendations

**Typical Duration:** 15-20 minutes

##### 5. OWASP ZAP Scanning (`.github/workflows/security-scan-zap.yml`)

**Purpose:** Dynamic application security testing (DAST) against running applications.

**Triggers:**
- Weekly on Sundays at 2 AM UTC
- Manual workflow dispatch

**Scan Types:**
- **Baseline Scan** - Passive scanning, safe for production
- **Full Scan** - Active scanning with attacks (staging only)
- **API Scan** - OpenAPI/Swagger specification-based testing

**Vulnerabilities Detected:**
- SQL injection
- Cross-site scripting (XSS)
- Security misconfigurations
- Broken authentication
- Sensitive data exposure
- Missing security headers

**Reports Generated:**
- HTML report for human review
- JSON report for automation
- Markdown report for GitHub issues

**Typical Duration:** 20-30 minutes (baseline), 45-60 minutes (full scan)

##### 6. Staging Deployment (`.github/workflows/deploy-staging.yml`)

**Purpose:** Automated deployment to staging environment for testing and validation.

**Triggers:**
- Automatic on push to `develop` branch
- Manual workflow dispatch

**Deployment Steps:**
1. Verify Docker images exist in ACR
2. Login to Azure with service principal
3. Initialize and apply Terraform configuration
4. Deploy API container to Azure Container Apps
5. Deploy Frontend container to Azure Container Apps
6. Retrieve SQL connection string from Key Vault
7. Run EF Core database migrations
8. Wait for applications to be ready
9. Run health checks and smoke tests
10. Notify deployment status

**Environment Configuration:**
- Resource Group: `rg-portfoliocms-staging`
- Container Apps Environment: `cae-portfoliocms-staging`
- SQL Database: Azure SQL Database (Free tier)
- Key Vault: `kv-portfoliocms-staging`

**Typical Duration:** 8-12 minutes

##### 7. Production Deployment (`.github/workflows/deploy-production.yml`)

**Purpose:** Controlled deployment to production with manual approval and comprehensive validation.

**Triggers:**
- Manual approval required
- Workflow dispatch only

**Safety Features:**
- ✅ Manual approval gate
- ✅ Staging validation before production
- ✅ Blue-green deployment support
- ✅ Automated rollback on failure
- ✅ Health check validation
- ✅ Zero-downtime deployment

**Deployment Process:**
1. Manual approval gate
2. Verify staging deployment success
3. Deploy infrastructure with Terraform
4. Deploy containers to production
5. Run database migrations
6. Execute comprehensive smoke tests
7. Monitor deployment health
8. Rollback capability if issues detected

**Typical Duration:** 15-20 minutes

##### 8. Infrastructure Deployment (`.github/workflows/infrastructure-deploy.yml`)

**Purpose:** Deploy and manage Azure infrastructure using Terraform.

**Triggers:**
- Manual workflow dispatch only

**Infrastructure Components:**
- Azure Container Apps Environment
- Azure SQL Database
- Azure Key Vault
- Azure Container Registry
- Application Insights
- Log Analytics Workspace

**Typical Duration:** 10-15 minutes

### Azure Container Apps Deployment

The recommended deployment uses Azure Container Apps with .NET Aspire orchestration:

**Architecture:**
```
┌─────────────────────────────────────────┐
│   Azure Container Apps Environment      │
│  ┌────────────┐      ┌────────────┐    │
│  │  Frontend  │ ───→ │    API     │    │
│  │ Container  │      │ Container  │    │
│  └────────────┘      └────────────┘    │
│         │                   │           │
│         └───────┬───────────┘           │
│                 ↓                       │
│  ┌──────────────────────────────────┐  │
│  │  Azure SQL Database (Free tier)  │  │
│  └──────────────────────────────────┘  │
│  ┌──────────────────────────────────┐  │
│  │  Azure Key Vault (Secrets)       │  │
│  └──────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

**Benefits:**
- ✅ Automatic scaling based on load
- ✅ Built-in service discovery
- ✅ Zero-downtime deployments
- ✅ Integrated monitoring with Application Insights
- ✅ Managed SSL certificates
- ✅ Cost-effective (pay per use)

### Prerequisites for Azure Deployment

1. **Azure CLI** - [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **Docker** - [Install Docker Desktop](https://www.docker.com/products/docker-desktop)
3. **Terraform** - [Install Terraform](https://www.terraform.io/downloads)
4. **Active Azure Subscription** with appropriate permissions

### 🔐 Critical: Key Vault Setup (Required Before Deployment)

⚠️ **IMPORTANT**: Before deploying infrastructure, you must manually create Azure Key Vault and store the database password. This is a security best practice to avoid storing sensitive credentials in code or Terraform state.

#### Step 1: Generate Secure Database Password

```bash
# Windows PowerShell
$password = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 16 | ForEach-Object {[char]$_})
echo $password

# Save this password securely - you'll need it for the next steps
```

#### Step 2: Create Key Vault and Store Credentials

```bash
# Login to Azure
az login

# Set variables (customize these)
$RESOURCE_GROUP="rg-portfoliocms-dev"
$LOCATION="eastus"
$KEY_VAULT_NAME="kv-portfoliocms-dev"  # Must be globally unique
$SQL_ADMIN_PASSWORD="<your-secure-password-from-step-1>"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Key Vault
az keyvault create `
  --name $KEY_VAULT_NAME `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --enable-rbac-authorization false `
  --enabled-for-deployment true `
  --enabled-for-template-deployment true

# Store SQL admin password in Key Vault
az keyvault secret set `
  --vault-name $KEY_VAULT_NAME `
  --name "SqlAdminPassword" `
  --value $SQL_ADMIN_PASSWORD

# Verify the secret was created
az keyvault secret show `
  --vault-name $KEY_VAULT_NAME `
  --name "SqlAdminPassword" `
  --query "value" `
  --output tsv
```

#### Step 3: Grant Your Account Access to Key Vault

```bash
# Get your Azure AD user object ID
$USER_OBJECT_ID=$(az ad signed-in-user show --query id --output tsv)

# Grant yourself access to manage secrets
az keyvault set-policy `
  --name $KEY_VAULT_NAME `
  --object-id $USER_OBJECT_ID `
  --secret-permissions get list set delete purge recover
```

### 🚀 Deploy Infrastructure with Terraform

After completing the Key Vault setup:

```bash
# Navigate to infrastructure directory
cd infrastructure

# Initialize Terraform
tofu init

# Retrieve password from Key Vault and set as environment variable
$SQL_PASSWORD=$(az keyvault secret show `
  --vault-name "kv-portfoliocms-dev" `
  --name "SqlAdminPassword" `
  --query "value" `
  --output tsv)
$env:TF_VAR_sql_admin_password=$SQL_PASSWORD

# Plan deployment (development environment)
tofu plan -var-file="environments/development/terraform.tfvars"

# Apply deployment
tofu apply -var-file="environments/development/terraform.tfvars"
```

### 🔑 Configure Managed Identity Access

After Terraform creates the App Services, grant them access to Key Vault:

```bash
# Get the App Service managed identity principal IDs
$API_PRINCIPAL_ID=$(az webapp identity show `
  --name "app-portfoliocms-api-dev" `
  --resource-group "rg-portfoliocms-dev" `
  --query principalId `
  --output tsv)

$FRONTEND_PRINCIPAL_ID=$(az webapp identity show `
  --name "app-portfoliocms-frontend-dev" `
  --resource-group "rg-portfoliocms-dev" `
  --query principalId `
  --output tsv)

# Grant API App Service access to Key Vault secrets
az keyvault set-policy `
  --name "kv-portfoliocms-dev" `
  --object-id $API_PRINCIPAL_ID `
  --secret-permissions get list

# Grant Frontend App Service access to Key Vault secrets
az keyvault set-policy `
  --name "kv-portfoliocms-dev" `
  --object-id $FRONTEND_PRINCIPAL_ID `
  --secret-permissions get list
```

### 📋 Azure Resources Created

The deployment creates the following Azure resources:

- **App Service Plan** (B1 tier for development)
- **App Services** (2) - Frontend and API with system-assigned managed identity
- **Azure SQL Database** - Free tier (32 MB storage, $0/month)
- **SQL Server** - Database server
- **Key Vault** - Stores database passwords and connection strings
- **Storage Account** - Media file storage
- **Application Insights** - Application monitoring
- **Log Analytics Workspace** - Centralized logging

**Estimated Monthly Cost (Development):** ~$15-20/month

### 🔄 Rotating Database Password

To rotate the database password securely:

```bash
# 1. Generate new password
$NEW_PASSWORD = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 16 | ForEach-Object {[char]$_})

# 2. Update SQL Server password
az sql server update `
  --resource-group "rg-portfoliocms-dev" `
  --name "sql-portfoliocms-dev" `
  --admin-password $NEW_PASSWORD

# 3. Update Key Vault secret
az keyvault secret set `
  --vault-name "kv-portfoliocms-dev" `
  --name "SqlAdminPassword" `
  --value $NEW_PASSWORD

# 4. Restart App Services to pick up new password
az webapp restart --name "app-portfoliocms-api-dev" --resource-group "rg-portfoliocms-dev"
az webapp restart --name "app-portfoliocms-frontend-dev" --resource-group "rg-portfoliocms-dev"
```

### 📚 Detailed Infrastructure Documentation

For complete infrastructure documentation, including:
- Detailed Azure CLI commands
- Troubleshooting guide
- Cost optimization tips
- Environment management
- CI/CD pipeline setup

See: **[infrastructure/README.md](infrastructure/README.md)**

---

## 🔄 CI/CD Pipeline Documentation

### Overview

The Portfolio CMS uses GitHub Actions for comprehensive CI/CD automation, including build, test, security scanning, containerization, and deployment to Azure. The pipeline follows industry best practices with quality gates, security scanning, and automated deployments.

### Pipeline Architecture

#### 1. Continuous Integration (CI) Pipeline

**Trigger:** Every push and pull request to `main` or `develop` branches

**Jobs:**
- **Build and Test** - Compiles solution and runs unit tests with code coverage
- **Security Scan** - Runs multiple security scanning tools
- **Infrastructure Validation** - Validates Terraform configurations
- **Code Quality Analysis** - Analyzes code quality metrics

**Workflow File:** `.github/workflows/ci.yml`

```bash
# What happens on every push/PR:
1. Checkout code
2. Setup .NET 9.0
3. Restore NuGet packages (with caching)
4. Build solution in Release mode
5. Run unit tests with code coverage
6. Upload coverage to Codecov
7. Run security scans (SonarCloud, Snyk, CodeQL, OWASP Dependency Check)
8. Validate Terraform infrastructure code
9. Generate code quality metrics
```

**Key Features:**
- ✅ Parallel job execution for faster feedback
- ✅ NuGet package caching for improved performance
- ✅ Code coverage reporting with Codecov integration
- ✅ Automated security vulnerability detection
- ✅ Infrastructure validation before deployment

#### 2. Docker Build and Push Pipeline

**Trigger:** Push to `main` or `develop` branches (after successful CI)

**Jobs:**
- **Build and Test** - Validates solution before containerization
- **Build API Image** - Creates and pushes API Docker image
- **Build Frontend Image** - Creates and pushes Frontend Docker image
- **Create Deployment Manifest** - Generates deployment metadata

**Workflow File:** `.github/workflows/docker-build-push.yml`

```bash
# Docker build process:
1. Run full test suite
2. Build Docker images with BuildKit caching
3. Tag images with commit SHA and 'latest'
4. Push to Azure Container Registry
5. Scan images with Trivy for vulnerabilities
6. Upload security scan results to GitHub Security
7. Create deployment manifest with image tags
```

**Image Tags:**
- `{commit-sha}` - Specific commit version (e.g., `abc123def`)
- `latest` - Most recent build
- `{branch}-{commit-sha}` - Branch-specific version

**Security Features:**
- 🔒 Trivy container vulnerability scanning
- 🔒 SARIF results uploaded to GitHub Security tab
- 🔒 Build cache optimization for faster builds
- 🔒 Multi-stage Docker builds for minimal image size

#### 3. Security Scanning Pipeline

**Trigger:** Daily at 2 AM UTC, or manual dispatch

**Jobs:**
- **Dependency Scan** - Scans for vulnerable dependencies
- **Static Code Analysis** - Security-focused code analysis
- **OWASP ZAP Scan** - Dynamic application security testing
- **Container Security Scan** - Scans Docker images
- **SSL/TLS Scan** - Validates SSL/TLS configuration
- **Security Headers Check** - Validates HTTP security headers

**Workflow File:** `.github/workflows/security-scan.yml`

```bash
# Security scanning tools:
- Snyk: Dependency vulnerability scanning
- OWASP Dependency Check: Known vulnerability detection
- GitHub CodeQL: Security-focused static analysis
- OWASP ZAP: Dynamic application security testing
- Trivy: Container image vulnerability scanning
- testssl.sh: SSL/TLS configuration validation
```

**Security Checks:**
- 🛡️ SQL injection vulnerabilities
- 🛡️ Cross-site scripting (XSS)
- 🛡️ Authentication and authorization issues
- 🛡️ Cryptographic weaknesses
- 🛡️ Dependency vulnerabilities (CVEs)
- 🛡️ Container image vulnerabilities
- 🛡️ SSL/TLS misconfigurations
- 🛡️ Missing security headers

#### 4. SonarCloud Analysis Pipeline

**Trigger:** Push/PR to `main` or `develop` branches

**Workflow File:** `.github/workflows/sonarcloud.yml`

```bash
# SonarCloud analysis:
1. Install SonarCloud scanner
2. Begin analysis with project configuration
3. Build solution with instrumentation
4. Run tests with OpenCover code coverage
5. End analysis and upload results
6. Wait for quality gate result
```

**Quality Gates:**
- Code Coverage: > 80%
- Duplicated Lines: < 3%
- Maintainability Rating: A
- Reliability Rating: A
- Security Rating: A
- Security Hotspots: Reviewed

**Exclusions:**
- Migration files
- DTOs and entity models
- Program.cs and Startup.cs
- Generated code

#### 5. OWASP ZAP Security Testing

**Trigger:** Weekly on Sundays at 2 AM UTC, or manual dispatch

**Workflow File:** `.github/workflows/security-scan-zap.yml`

```bash
# ZAP scanning modes:
- Baseline Scan: Quick passive scan for common issues
- Full Scan: Comprehensive active scan (longer duration)
- API Scan: OpenAPI/Swagger specification-based testing
```

**Scan Types:**
- **Baseline Scan** - Passive scanning, no attacks (safe for production)
- **Full Scan** - Active scanning with attacks (staging only)
- **API Scan** - Tests API endpoints using OpenAPI spec

**Reports Generated:**
- HTML report for human review
- JSON report for automation
- Markdown report for GitHub issues

#### 6. Staging Deployment Pipeline

**Trigger:** Automatic on push to `develop` branch

**Jobs:**
- **Verify Docker Images** - Ensures images exist in ACR
- **Deploy Infrastructure** - Applies Terraform changes
- **Deploy to Container Apps** - Updates Azure Container Apps
- **Run Database Migrations** - Applies EF Core migrations
- **Post-deployment Tests** - Validates deployment health

**Workflow File:** `.github/workflows/deploy-staging.yml`

```bash
# Staging deployment process:
1. Verify Docker images exist in ACR
2. Login to Azure with service principal
3. Initialize and apply Terraform configuration
4. Deploy API container to Azure Container Apps
5. Deploy Frontend container to Azure Container Apps
6. Retrieve SQL connection string from Key Vault
7. Run EF Core database migrations
8. Wait for applications to be ready
9. Run health checks and smoke tests
10. Notify deployment status
```

**Environment Configuration:**
- Resource Group: `rg-portfoliocms-staging`
- Container Apps Environment: `cae-portfoliocms-staging`
- SQL Database: Azure SQL Database (Free tier)
- Key Vault: `kv-portfoliocms-staging`

#### 7. Production Deployment Pipeline

**Trigger:** Manual approval required

**Workflow File:** `.github/workflows/deploy-production.yml`

**Deployment Process:**
1. Manual approval gate
2. Verify staging deployment success
3. Deploy infrastructure with Terraform
4. Deploy containers to production
5. Run database migrations
6. Execute comprehensive smoke tests
7. Monitor deployment health
8. Rollback capability if issues detected

**Production Safety Features:**
- ✅ Manual approval required
- ✅ Staging validation before production
- ✅ Blue-green deployment support
- ✅ Automated rollback on failure
- ✅ Health check validation
- ✅ Zero-downtime deployment

### Setting Up CI/CD

#### Prerequisites

1. **Azure Resources**
   - Azure subscription with Owner or Contributor role
   - Azure Container Registry (Basic SKU minimum)
   - Azure Container Apps or App Services
   - Azure SQL Database (Free tier for development)
   - Azure Key Vault for secrets management

2. **GitHub Repository**
   - Repository with GitHub Actions enabled
   - Admin access to configure secrets
   - Branch protection rules (recommended)

3. **Third-Party Services (Optional but Recommended)**
   - SonarCloud account for code quality analysis
   - Snyk account for dependency scanning
   - Grafana Cloud account for monitoring

#### Required GitHub Secrets

Configure the following secrets in your GitHub repository (Settings → Secrets and variables → Actions):

| Secret Name | Description | How to Get | Required |
|-------------|-------------|------------|----------|
| `ACR_USERNAME` | Azure Container Registry username | `az acr credential show --name <acr-name> --query username -o tsv` | ✅ Yes |
| `ACR_PASSWORD` | Azure Container Registry password | `az acr credential show --name <acr-name> --query passwords[0].value -o tsv` | ✅ Yes |
| `AZURE_CREDENTIALS` | Azure service principal credentials (JSON) | See Step 1 below | ✅ Yes |
| `ARM_CLIENT_ID` | Azure service principal client ID | From service principal creation | ✅ Yes |
| `ARM_CLIENT_SECRET` | Azure service principal client secret | From service principal creation | ✅ Yes |
| `ARM_SUBSCRIPTION_ID` | Azure subscription ID | `az account show --query id -o tsv` | ✅ Yes |
| `ARM_TENANT_ID` | Azure tenant ID | `az account show --query tenantId -o tsv` | ✅ Yes |
| `SQL_ADMIN_PASSWORD` | SQL Server admin password | Generate secure password | ✅ Yes |
| `SONAR_TOKEN` | SonarCloud authentication token | SonarCloud → My Account → Security | ⚠️ Optional |
| `SNYK_TOKEN` | Snyk API token | Snyk → Account Settings → API Token | ⚠️ Optional |

#### Step-by-Step Setup Guide

##### Step 1: Create Azure Service Principal

Create a service principal for GitHub Actions to authenticate with Azure:

```bash
# Login to Azure
az login

# Get your subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
echo "Subscription ID: $SUBSCRIPTION_ID"

# Create service principal with Contributor role
az ad sp create-for-rbac \
  --name "sp-portfoliocms-github-actions" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth

# Output will be JSON - save this entire JSON as AZURE_CREDENTIALS secret
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

**Important:** 
- Copy the entire JSON output and save it as the `AZURE_CREDENTIALS` secret
- Also save `clientId` as `ARM_CLIENT_ID` secret
- Save `clientSecret` as `ARM_CLIENT_SECRET` secret
- Save `subscriptionId` as `ARM_SUBSCRIPTION_ID` secret
- Save `tenantId` as `ARM_TENANT_ID` secret

##### Step 2: Configure Azure Container Registry

Set up Azure Container Registry for storing Docker images:

```bash
# Set variables
RESOURCE_GROUP="rg-portfoliocms-shared"
LOCATION="eastus"
ACR_NAME="acrportfoliocms"  # Must be globally unique

# Create resource group (if not exists)
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Azure Container Registry
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true

# Get ACR credentials
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query username -o tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv)

echo "ACR Username: $ACR_USERNAME"
echo "ACR Password: $ACR_PASSWORD"

# Add these as ACR_USERNAME and ACR_PASSWORD secrets in GitHub
```

##### Step 3: Configure SonarCloud (Optional but Recommended)

Set up SonarCloud for static code analysis:

1. Go to [https://sonarcloud.io](https://sonarcloud.io)
2. Sign in with your GitHub account
3. Click "+" → "Analyze new project"
4. Select your repository
5. Choose "With GitHub Actions" as the analysis method
6. Generate a token:
   - Go to My Account → Security
   - Generate a new token
   - Copy the token
7. Add the token as `SONAR_TOKEN` secret in GitHub
8. Update `.github/workflows/sonarcloud.yml`:
   ```yaml
   /o:"your-organization-key"  # Replace with your SonarCloud organization
   ```

**Benefits of SonarCloud:**
- Automated code quality analysis
- Security vulnerability detection
- Technical debt tracking
- Code coverage visualization
- Quality gate enforcement

##### Step 4: Configure Snyk (Optional but Recommended)

Set up Snyk for dependency vulnerability scanning:

1. Go to [https://snyk.io](https://snyk.io)
2. Sign up with your GitHub account
3. Go to Account Settings → API Token
4. Click "Generate" to create a new token
5. Copy the token
6. Add the token as `SNYK_TOKEN` secret in GitHub

**Benefits of Snyk:**
- Real-time dependency vulnerability scanning
- Automated fix pull requests
- License compliance checking
- Container image scanning

##### Step 5: Update Workflow Configuration

Update environment-specific values in workflow files:

**1. Update ACR name in `.github/workflows/docker-build-push.yml`:**
```yaml
env:
  REGISTRY_NAME: 'acrportfoliocms'  # Replace with your ACR name
```

**2. Update ACR name in `.github/workflows/deploy-staging.yml`:**
```yaml
env:
  REGISTRY_NAME: 'acrportfoliocms'  # Replace with your ACR name
  RESOURCE_GROUP: 'rg-portfoliocms-staging'
  CONTAINER_APP_ENV: 'cae-portfoliocms-staging'
```

**3. Update SonarCloud organization in `.github/workflows/sonarcloud.yml`:**
```yaml
- name: Begin SonarCloud analysis
  run: |
    dotnet sonarscanner begin \
      /k:"PortfolioCMS" \
      /o:"your-organization-key" \  # Replace with your SonarCloud organization
```

**4. Update application URLs in `.github/workflows/security-scan-zap.yml`:**
```yaml
env:
  STAGING_FRONTEND_URL: 'https://your-staging-app.azurewebsites.net'
  STAGING_API_URL: 'https://your-staging-api.azurewebsites.netzurewebsites.net'
```

### Monitoring CI/CD Pipeline

#### GitHub Actions Dashboard

View pipeline status:
1. Go to your repository on GitHub
2. Click "Actions" tab
3. View workflow runs, logs, and artifacts

#### Pipeline Artifacts

Each workflow generates artifacts:
- **Test Results** - TRX files with test execution details
- **Code Coverage** - Coverage reports and metrics
- **Security Scan Reports** - OWASP ZAP, Trivy, Dependency Check reports
- **Deployment Manifests** - Image tags and deployment metadata

#### Viewing Security Scan Results

**GitHub Security Tab:**
1. Go to repository → Security tab
2. View Code scanning alerts (CodeQL, Trivy)
3. View Dependabot alerts
4. View Secret scanning alerts

**SonarCloud Dashboard:**
1. Go to https://sonarcloud.io
2. Select your project
3. View quality gates, code smells, vulnerabilities

##### Step 6: Commit and Push Configuration Changes

After updating the workflow files:

```bash
# Add the modified workflow files
git add .github/workflows/

# Commit the changes
git commit -m "Configure CI/CD pipelines with Azure resources"

# Push to repository
git push origin main
```

##### Step 7: Verify Pipeline Setup

Test your CI/CD setup:

```bash
# 1. Trigger CI pipeline
git checkout -b test/ci-setup
echo "# Test CI" >> README.md
git add README.md
git commit -m "Test CI pipeline"
git push origin test/ci-setup

# 2. Create pull request and verify:
# - CI pipeline runs successfully
# - Tests pass
# - Code quality checks pass
# - Security scans complete

# 3. Merge PR to trigger Docker build
# 4. Push to develop branch to trigger staging deployment
```

#### Monitoring Pipeline Execution

##### GitHub Actions Dashboard

View and monitor pipeline execution:

1. Go to your repository on GitHub
2. Click the "Actions" tab
3. View workflow runs, logs, and artifacts
4. Click on any workflow run to see detailed logs
5. Download artifacts for offline analysis

##### Pipeline Status Badges

Add status badges to your README:

```markdown
![CI Pipeline](https://github.com/your-username/portfolio-cms/actions/workflows/ci.yml/badge.svg)
![Docker Build](https://github.com/your-username/portfolio-cms/actions/workflows/docker-build-push.yml/badge.svg)
![Security Scan](https://github.com/your-username/portfolio-cms/actions/workflows/security-scan.yml/badge.svg)
```

##### Viewing Artifacts

Each workflow generates artifacts that can be downloaded:

- **Test Results** - TRX files with test execution details
- **Code Coverage** - Coverage reports and metrics
- **Security Scan Reports** - OWASP ZAP, Trivy, Dependency Check reports
- **Deployment Manifests** - Image tags and deployment metadata
- **Build Logs** - Detailed build and deployment logs

To download artifacts:
1. Go to workflow run summary
2. Scroll to "Artifacts" section
3. Click on artifact name to download

##### Viewing Security Scan Results

**GitHub Security Tab:**
1. Go to repository → Security tab
2. View Code scanning alerts (CodeQL, Trivy)
3. View Dependabot alerts for vulnerable dependencies
4. View Secret scanning alerts (if enabled)

**SonarCloud Dashboard:**
1. Go to [https://sonarcloud.io](https://sonarcloud.io)
2. Select your project
3. View quality gates, code smells, vulnerabilities, and technical debt
4. Review code coverage and duplication metrics

**Snyk Dashboard:**
1. Go to [https://app.snyk.io](https://app.snyk.io)
2. Select your project
3. View dependency vulnerabilities
4. Review fix recommendations and automated PRs

### Troubleshooting CI/CD Issues

#### Common Issues and Solutions

##### 1. Build Failures

**Issue: NuGet restore fails**

```bash
# Solution: Clear NuGet cache
dotnet nuget locals all --clear

# Verify package sources
dotnet nuget list source

# Restore with verbose logging
dotnet restore --verbosity detailed
```

**Issue: Compilation errors**

```bash
# Solution: Build locally first
dotnet clean
dotnet restore
dotnet build --configuration Release

# Check for missing dependencies
dotnet list package --vulnerable
dotnet list package --outdated
```

**Issue: Test failures**

```bash
# Solution: Run tests locally with detailed output
dotnet test --verbosity detailed --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~YourTestName"

# Check test output for specific failures
dotnet test --logger "trx;LogFileName=test-results.trx"
```

##### 2. Docker Build Failures

**Issue: Docker build fails with "no space left on device"**

```bash
# Solution: Clean up Docker resources
docker system prune -a --volumes

# Remove unused images
docker image prune -a

# Check disk space
docker system df
```

**Issue: Cannot push to ACR**

```bash
# Solution: Verify ACR credentials
az acr credential show --name acrportfoliocms

# Test ACR login
docker login acrportfoliocms.azurecr.io

# Verify ACR admin user is enabled
az acr update --name acrportfoliocms --admin-enabled true

# Check GitHub secrets are correct
# - ACR_USERNAME should match ACR username
# - ACR_PASSWORD should match ACR password
```

**Issue: Dockerfile COPY fails**

```bash
# Solution: Ensure correct build context
# Build from repository root, not from src/ directory
docker build -f src/PortfolioCMS.API/Dockerfile .

# Verify .dockerignore is not excluding required files
cat .dockerignore
```

**Issue: Multi-stage build fails**

```bash
# Solution: Build stages individually to identify issue
docker build --target build -f src/PortfolioCMS.API/Dockerfile .
docker build --target publish -f src/PortfolioCMS.API/Dockerfile .

# Check base image availability
docker pull mcr.microsoft.com/dotnet/aspnet:9.0
docker pull mcr.microsoft.com/dotnet/sdk:9.0
```

##### 3. Deployment Failures

**Issue: Terraform apply fails**

```bash
# Solution: Check Azure credentials
# Verify ARM_* secrets are correct in GitHub
az login --service-principal \
  --username $ARM_CLIENT_ID \
  --password $ARM_CLIENT_SECRET \
  --tenant $ARM_TENANT_ID

# Check service principal has Contributor role
az role assignment list --assignee $ARM_CLIENT_ID

# Verify subscription ID is correct
az account show --query id -o tsv

# Initialize Terraform with verbose logging
terraform init -upgrade
terraform plan -var-file='environments/staging/terraform.tfvars'
```

**Issue: Container app deployment fails**

```bash
# Solution: Check container app logs
az containerapp logs show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --follow

# Verify image exists in ACR
az acr repository show \
  --name acrportfoliocms \
  --repository portfoliocms-api

# Check environment variables
az containerapp show \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging \
  --query properties.template.containers[0].env

# Verify container app revision status
az containerapp revision list \
  --name ca-portfoliocms-api-staging \
  --resource-group rg-portfoliocms-staging
```

**Issue: Database migration fails**

```bash
# Solution: Check connection string
# Verify SQL_ADMIN_PASSWORD secret is correct
az keyvault secret show \
  --vault-name kv-portfoliocms-staging \
  --name SqlConnectionString

# Check Key Vault access policies
az keyvault show \
  --name kv-portfoliocms-staging \
  --query properties.accessPolicies

# Test connection string manually
dotnet ef database update \
  --project src/PortfolioCMS.DataAccess \
  --startup-project src/PortfolioCMS.API \
  --connection "Server=..." \
  --verbose

# Check SQL Server firewall rules
az sql server firewall-rule list \
  --server sql-portfoliocms-staging \
  --resource-group rg-portfoliocms-staging
```

##### 4. Security Scan Failures

**Issue: SonarCloud quality gate fails**

```bash
# Solution: Review and fix issues
# 1. Check SonarCloud dashboard for specific issues
# 2. Fix code smells, bugs, and vulnerabilities
# 3. Improve code coverage if below threshold

# Run SonarCloud analysis locally
dotnet sonarscanner begin \
  /k:"PortfolioCMS" \
  /d:sonar.token="your-token"
dotnet build
dotnet sonarscanner end /d:sonar.token="your-token"

# Update quality gate thresholds if needed (in SonarCloud UI)
```

**Issue: OWASP ZAP finds vulnerabilities**

```bash
# Solution: Review and remediate
# 1. Download ZAP report from workflow artifacts
# 2. Review findings and severity levels
# 3. Implement fixes for high/critical issues

# Suppress false positives in .zap/rules.tsv
echo "10021	IGNORE	(False Positive - Explanation)" >> .zap/rules.tsv

# Test fixes locally with ZAP Docker
docker run -t owasp/zap2docker-stable zap-baseline.py \
  -t https://your-app-url \
  -r zap-report.html
```

**Issue: Trivy finds container vulnerabilities**

```bash
# Solution: Update base images and dependencies
# 1. Update .NET base image in Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine  # Use alpine for smaller attack surface

# 2. Update NuGet packages
dotnet list package --outdated
dotnet add package PackageName --version x.x.x

# 3. Rebuild and rescan
docker build -f src/PortfolioCMS.API/Dockerfile .
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy image your-image:tag

# 4. Review SARIF results in GitHub Security tab
```

**Issue: Snyk finds dependency vulnerabilities**

```bash
# Solution: Update vulnerable dependencies
# 1. View Snyk dashboard for details
# 2. Update packages to patched versions
dotnet add package VulnerablePackage --version x.x.x

# 3. Test for breaking changes
dotnet test

# 4. Commit and push updates
git add *.csproj
git commit -m "Update vulnerable dependencies"
git push
```

##### 5. Authentication Issues

**Issue: Azure login fails in workflow**

```bash
# Solution: Recreate service principal
# 1. Delete old service principal
az ad sp delete --id $ARM_CLIENT_ID

# 2. Create new service principal
az ad sp create-for-rbac \
  --name "sp-portfoliocms-github-actions" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth

# 3. Update AZURE_CREDENTIALS secret in GitHub
# 4. Update ARM_CLIENT_ID, ARM_CLIENT_SECRET secrets
```

**Issue: ACR authentication fails**

```bash
# Solution: Regenerate ACR credentials
az acr credential renew \
  --name acrportfoliocms \
  --password-name password

# Get new credentials
az acr credential show --name acrportfoliocms

# Update ACR_PASSWORD secret in GitHub
```

**Issue: Key Vault access denied**

```bash
# Solution: Grant access to service principal
# Get service principal object ID
SP_OBJECT_ID=$(az ad sp show --id $ARM_CLIENT_ID --query id -o tsv)

# Grant Key Vault access
az keyvault set-policy \
  --name kv-portfoliocms-staging \
  --object-id $SP_OBJECT_ID \
  --secret-permissions get list

# Verify access
az keyvault secret show \
  --vault-name kv-portfoliocms-staging \
  --name SqlConnectionString
```

##### 6. Performance Issues

**Issue: Workflow runs too slowly**

```bash
# Solution: Optimize workflow
# 1. Enable caching for dependencies
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}

# 2. Use matrix builds for parallel execution
strategy:
  matrix:
    project: [API, Frontend, DataAccess]

# 3. Skip unnecessary steps with conditions
if: github.event_name == 'push'

# 4. Use self-hosted runners for better performance
runs-on: self-hosted
```

#### Debugging Workflow Runs

##### Enable Debug Logging

Add the following secrets to your repository for detailed debugging:

| Secret Name | Value | Purpose |
|-------------|-------|---------|
| `ACTIONS_STEP_DEBUG` | `true` | Enables detailed step-by-step logs |
| `ACTIONS_RUNNER_DEBUG` | `true` | Enables runner diagnostic logs |

**To add debug secrets:**
1. Go to Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Add both secrets with value `true`
4. Re-run failed workflow to see detailed logs

##### View Detailed Logs

**Method 1: GitHub UI**
1. Go to Actions tab
2. Select the failed workflow run
3. Click on the failed job
4. Expand the failed step
5. Review error messages and stack traces
6. Use search (Ctrl+F) to find specific errors

**Method 2: GitHub CLI**
```bash
# Install GitHub CLI
# https://cli.github.com/

# View workflow runs
gh run list --workflow=ci.yml

# View specific run logs
gh run view <run-id> --log

# Download logs
gh run download <run-id>
```

##### Download and Analyze Artifacts

**Via GitHub UI:**
1. Go to workflow run summary page
2. Scroll to "Artifacts" section at the bottom
3. Click on artifact name to download
4. Extract and review contents

**Via GitHub CLI:**
```bash
# List artifacts for a run
gh run view <run-id> --log

# Download all artifacts
gh run download <run-id>

# Download specific artifact
gh run download <run-id> --name test-results
```

##### Common Log Patterns to Search For

When debugging, search logs for these patterns:

```bash
# Errors
"error:"
"Error:"
"ERROR"
"failed"
"Failed"
"FAILED"

# Warnings
"warning:"
"Warning:"
"WARN"

# Exceptions
"Exception"
"StackTrace"
"at System."
"at Microsoft."

# Authentication issues
"401"
"403"
"Unauthorized"
"Forbidden"
"authentication failed"

# Network issues
"timeout"
"connection refused"
"network error"
"DNS"
```

##### Re-running Failed Workflows

**Re-run entire workflow:**
1. Go to failed workflow run
2. Click "Re-run all jobs" button
3. Optionally enable debug logging first

**Re-run specific job:**
1. Go to failed workflow run
2. Click on the failed job
3. Click "Re-run job" button

**Re-run via GitHub CLI:**
```bash
# Re-run failed jobs only
gh run rerun <run-id> --failed

# Re-run all jobs
gh run rerun <run-id>
```

#### Getting Help and Support

##### Documentation Resources

**GitHub Actions:**
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Workflow Syntax Reference](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions)
- [GitHub Actions Marketplace](https://github.com/marketplace?type=actions)

**Azure Services:**
- [Azure Container Apps Documentation](https://docs.microsoft.com/en-us/azure/container-apps/)
- [Azure Container Registry Documentation](https://docs.microsoft.com/en-us/azure/container-registry/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)

**Infrastructure as Code:**
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Terraform Documentation](https://www.terraform.io/docs)

**Security Tools:**
- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [OWASP ZAP Documentation](https://www.zaproxy.org/docs/)
- [Snyk Documentation](https://docs.snyk.io/)
- [Trivy Documentation](https://aquasecurity.github.io/trivy/)

**.NET and Development:**
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)

##### Support Channels

**Project-Specific Issues:**
- Create a GitHub Issue in this repository
- Include workflow run ID and relevant logs
- Describe steps to reproduce the issue

**General CI/CD Questions:**
- [Stack Overflow - GitHub Actions](https://stackoverflow.com/questions/tagged/github-actions)
- [Stack Overflow - Azure DevOps](https://stackoverflow.com/questions/tagged/azure-devops)
- [Stack Overflow - Terraform](https://stackoverflow.com/questions/tagged/terraform)

**Azure Support:**
- [Azure Support Portal](https://portal.azure.com/#blade/Microsoft_Azure_Support/HelpAndSupportBlade)
- [Azure Community Forums](https://docs.microsoft.com/en-us/answers/products/azure)
- [Azure Stack Overflow](https://stackoverflow.com/questions/tagged/azure)

**Security Tool Support:**
- [SonarCloud Community](https://community.sonarsource.com/)
- [Snyk Support](https://support.snyk.io/)
- [OWASP ZAP User Group](https://groups.google.com/g/zaproxy-users)

##### Reporting Security Issues

If you discover a security vulnerability in the CI/CD pipeline or application:

1. **DO NOT** create a public GitHub issue
2. Email security concerns to: [security@your-domain.com]
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)
4. Allow 90 days for response and remediation before public disclosure

See [SECURITY.md](SECURITY.md) for complete security policy

### Pipeline Performance Optimization

#### Caching Strategies

The CI/CD pipelines implement multiple caching layers to improve performance:

**1. NuGet Package Caching**
```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```
**Benefit:** Reduces package restore time from 2-3 minutes to 10-20 seconds

**2. Docker Layer Caching**
```yaml
- uses: docker/build-push-action@v5
  with:
    cache-from: type=registry,ref=${{ env.REGISTRY_NAME }}.azurecr.io/portfoliocms-api:buildcache
    cache-to: type=registry,ref=${{ env.REGISTRY_NAME }}.azurecr.io/portfoliocms-api:buildcache,mode=max
```
**Benefit:** Reduces Docker build time from 8-10 minutes to 3-5 minutes

**3. SonarCloud Scanner Caching**
```yaml
- uses: actions/cache@v4
  with:
    path: ~/.sonar/cache
    key: ${{ runner.os }}-sonar
```
**Benefit:** Reduces SonarCloud analysis time by 30-40%

**4. Terraform Provider Caching**
```yaml
- uses: actions/cache@v4
  with:
    path: ~/.terraform.d/plugin-cache
    key: ${{ runner.os }}-terraform-${{ hashFiles('**/.terraform.lock.hcl') }}
```
**Benefit:** Reduces Terraform init time from 1-2 minutes to 10-15 seconds

#### Parallel Execution

Jobs are configured to run in parallel where possible:

```yaml
jobs:
  build-api-image:
    needs: build-and-test
    # Runs in parallel with build-frontend-image
  
  build-frontend-image:
    needs: build-and-test
    # Runs in parallel with build-api-image
  
  security-scan-dependencies:
    # Runs in parallel with other security scans
  
  security-scan-code:
    # Runs in parallel with other security scans
```

**Performance Gains:**
- Security scans complete in 15-20 minutes instead of 45-60 minutes
- Docker images build in 10-15 minutes instead of 20-30 minutes
- Overall pipeline time reduced by 40-50%

#### Optimization Tips

**1. Use Conditional Execution**
```yaml
# Skip deployment on PR
if: github.event_name != 'pull_request'

# Run only on specific file changes
paths:
  - 'src/**'
  - '**.csproj'
```

**2. Optimize Build Commands**
```bash
# Use --no-restore when packages are already restored
dotnet build --no-restore

# Use --no-build when running tests after build
dotnet test --no-build

# Use parallel builds
dotnet build -m
```

**3. Use Matrix Builds for Multi-Platform Testing**
```yaml
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest]
    dotnet: ['8.0.x', '9.0.x']
```

**4. Implement Smart Triggering**
```yaml
# Only run on relevant changes
on:
  push:
    paths:
      - 'src/**'
      - '**.csproj'
      - '.github/workflows/ci.yml'
```

**5. Use Self-Hosted Runners for Better Performance**
```yaml
runs-on: self-hosted  # Faster than GitHub-hosted runners
```

#### Performance Metrics

**Typical Pipeline Durations:**

| Pipeline | Without Optimization | With Optimization | Improvement |
|----------|---------------------|-------------------|-------------|
| CI Pipeline | 12-15 min | 5-8 min | 50-60% faster |
| Docker Build | 20-25 min | 10-15 min | 40-50% faster |
| Security Scan | 45-60 min | 15-20 min | 65-70% faster |
| Deployment | 15-20 min | 8-12 min | 40-45% faster |

### CI/CD Security Best Practices

#### Secrets Management

**✅ DO:**
- Store all sensitive data in GitHub Secrets
- Use Azure Key Vault for application secrets
- Rotate secrets regularly (every 90 days)
- Implement least privilege access
- Use environment-specific secrets
- Audit secret access regularly

**❌ DON'T:**
- Never commit secrets to repository
- Don't hardcode credentials in workflows
- Don't log secrets in workflow output
- Don't share secrets across environments
- Don't use personal access tokens for automation

**Example: Secure Secret Usage**
```yaml
- name: Login to Azure
  uses: azure/login@v1
  with:
    creds: ${{ secrets.AZURE_CREDENTIALS }}  # Stored in GitHub Secrets

- name: Get SQL Password from Key Vault
  run: |
    SQL_PASSWORD=$(az keyvault secret show \
      --vault-name ${{ secrets.KEY_VAULT_NAME }} \
      --name SqlAdminPassword \
      --query value -o tsv)
    echo "::add-mask::$SQL_PASSWORD"  # Mask in logs
    echo "SQL_PASSWORD=$SQL_PASSWORD" >> $GITHUB_ENV
```

#### Pipeline Security

**✅ DO:**
- Pin action versions to specific tags (e.g., `@v4`, not `@main`)
- Review third-party actions before use
- Enable branch protection rules
- Require status checks before merge
- Use environment protection rules for production
- Implement manual approval gates for production
- Enable Dependabot for action updates
- Use CODEOWNERS for workflow changes

**❌ DON'T:**
- Don't use actions from untrusted sources
- Don't disable security checks
- Don't allow direct pushes to main/production branches
- Don't skip code review for workflow changes

**Example: Branch Protection Configuration**
```yaml
# .github/settings.yml (using probot/settings)
branches:
  - name: main
    protection:
      required_status_checks:
        strict: true
        contexts:
          - "CI Pipeline"
          - "Security Scan"
          - "SonarCloud Analysis"
      required_pull_request_reviews:
        required_approving_review_count: 1
        dismiss_stale_reviews: true
      enforce_admins: true
      required_linear_history: true
```

#### Deployment Security

**✅ DO:**
- Require manual approval for production deployments
- Run automated security scanning before deployment
- Use immutable infrastructure with Terraform
- Implement zero-trust network architecture
- Use managed identities for Azure resources
- Enable audit logging for all deployments
- Implement automated rollback on failure
- Use blue-green or canary deployments

**❌ DON'T:**
- Don't deploy directly to production without testing
- Don't skip security scans for "urgent" deployments
- Don't use shared credentials across environments
- Don't disable audit logging

**Example: Production Deployment with Approval**
```yaml
jobs:
  deploy-production:
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://portfoliocms.com
    steps:
      - name: Manual Approval Required
        uses: trstringer/manual-approval@v1
        with:
          secret: ${{ secrets.GITHUB_TOKEN }}
          approvers: admin-team
          minimum-approvals: 2
          issue-title: "Deploy to Production"
```

#### Security Scanning Integration

**Comprehensive Security Coverage:**

| Tool | Purpose | Frequency | Severity Threshold |
|------|---------|-----------|-------------------|
| SonarCloud | Static code analysis | Every push/PR | High/Critical |
| Snyk | Dependency scanning | Daily | High/Critical |
| OWASP ZAP | Dynamic app security | Weekly | Medium+ |
| Trivy | Container scanning | Every build | High/Critical |
| CodeQL | Security code analysis | Every push/PR | High/Critical |
| Dependabot | Dependency updates | Daily | All |

**Security Gate Policy:**
- ❌ Block deployment if critical vulnerabilities found
- ⚠️ Warn on high severity issues (require review)
- ✅ Allow deployment with medium/low issues (track in backlog)

**Example: Security Gate Implementation**
```yaml
- name: Check Security Scan Results
  run: |
    CRITICAL_COUNT=$(jq '.vulnerabilities | map(select(.severity=="CRITICAL")) | length' scan-results.json)
    if [ $CRITICAL_COUNT -gt 0 ]; then
      echo "❌ Found $CRITICAL_COUNT critical vulnerabilities"
      echo "Deployment blocked until vulnerabilities are resolved"
      exit 1
    fi
```

#### Compliance and Audit

**Audit Trail Requirements:**
- ✅ Log all deployment activities
- ✅ Track who approved deployments
- ✅ Record configuration changes
- ✅ Monitor secret access
- ✅ Retain logs for compliance period (typically 90 days)

**Example: Audit Logging**
```yaml
- name: Log Deployment
  run: |
    cat > deployment-log.json << EOF
    {
      "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
      "environment": "production",
      "version": "${{ github.sha }}",
      "deployed_by": "${{ github.actor }}",
      "approved_by": "${{ github.event.review.user.login }}",
      "workflow_run": "${{ github.run_id }}"
    }
    EOF
    
    # Upload to audit log storage
    az storage blob upload \
      --account-name auditlogs \
      --container-name deployments \
      --file deployment-log.json \
      --name "$(date +%Y/%m/%d)/${{ github.run_id }}.json"
```

### Continuous Improvement

#### Monitoring Pipeline Health

**Key Metrics to Track:**
- Pipeline success rate (target: >95%)
- Average pipeline duration (track trends)
- Security scan findings (track reduction over time)
- Deployment frequency (track velocity)
- Mean time to recovery (MTTR)
- Change failure rate

**Tools for Monitoring:**
- GitHub Actions insights
- Azure Monitor dashboards
- Grafana Cloud dashboards
- Custom metrics collection

#### Regular Maintenance Tasks

**Weekly:**
- Review failed pipeline runs
- Update security scan suppressions
- Check for action updates

**Monthly:**
- Review and rotate secrets
- Update dependencies
- Review security scan trends
- Optimize slow pipelines

**Quarterly:**
- Audit access permissions
- Review and update security policies
- Evaluate new tools and actions
- Conduct security training

---

## 📁 Project Structure

```
portfolio-cms/
├── .kiro/                     # Kiro AI configuration
├── src/
│   ├── PortfolioCMS.API/      # RESTful API Backend
│   │   ├── Controllers/       # API endpoints
│   │   ├── Services/          # Business logic
│   │   ├── DTOs/              # Data transfer objects
│   │   └── Program.cs         # Application entry point
│   ├── PortfolioCMS.DataAccess/
│   │   ├── Context/           # Entity Framework DbContext
│   │   ├── Entities/          # Database models
│   │   ├── Repositories/      # Data access layer
│   │   └── Migrations/        # Database migrations
│   ├── PortfolioCMS.Frontend/ # Blazor WebAssembly
│   │   ├── Components/        # Reusable UI components
│   │   ├── Pages/             # Application pages
│   │   └── Services/          # Frontend services
│   └── Tests/                 # Unit and integration tests
├── README.md                  # This file
└── PortfolioCMS.sln          # Solution file
```

## 🔌 API Endpoints

The API provides comprehensive endpoints for content management:

### Authentication
- `POST /api/v1/auth/login` - User authentication
- `POST /api/v1/auth/register` - User registration
- `GET /api/v1/auth/profile` - Get user profile
- `PUT /api/v1/auth/profile` - Update user profile

### Articles
- `GET /api/v1/articles` - Get published articles (with pagination)
- `GET /api/v1/articles/{id}` - Get article by ID
- `GET /api/v1/articles/search` - Search articles
- `POST /api/v1/articles` - Create article (Admin only)
- `PUT /api/v1/articles/{id}` - Update article (Admin only)
- `DELETE /api/v1/articles/{id}` - Delete article (Admin only)

### Projects
- `GET /api/v1/projects` - Get active projects
- `GET /api/v1/projects/{id}` - Get project by ID
- `POST /api/v1/projects` - Create project (Admin only)
- `PUT /api/v1/projects/{id}` - Update project (Admin only)
- `DELETE /api/v1/projects/{id}` - Delete project (Admin only)

### Comments
- `POST /api/v1/comments` - Submit comment for moderation
- `GET /api/v1/comments/article/{articleId}` - Get approved comments
- `POST /api/v1/comments/{id}/approve` - Approve comment (Admin only)
- `POST /api/v1/comments/{id}/reject` - Reject comment (Admin only)

### Health & Monitoring
- `GET /health` - Application health check
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe
- `GET /api/v1/health/info` - System information

**📖 Complete API documentation available at:** http://localhost:5000/swagger

---

## 🌟 Features

### For Visitors
- **📖 Article Reading:** Browse and read technical articles
- **🔍 Search & Filter:** Find articles by keywords, tags, or categories
- **💬 Comments:** Submit comments on articles (with moderation)
- **🎨 Portfolio Viewing:** Explore showcased projects and work

### For Administrators
- **✍️ Content Management:** Create, edit, and publish articles
- **🏗️ Project Management:** Showcase portfolio projects
- **💬 Comment Moderation:** Approve, reject, or delete comments
- **📊 Analytics Dashboard:** View content and engagement statistics
- **🔐 User Management:** Manage user accounts and permissions
- **📁 Media Management:** Upload and organize images and files

### Technical Features
- **🔐 JWT Authentication:** Secure API access with role-based authorization
- **📊 Comprehensive Observability:** Application Insights and Grafana Cloud integration
- **🏥 Health Monitoring:** Built-in health checks for container orchestration
- **🔄 Database Migrations:** Automated database schema management
- **📝 API Documentation:** Auto-generated Swagger/OpenAPI documentation
- **🛡️ Security Scanning:** SonarCloud, Snyk, OWASP ZAP, CodeQL, and Trivy integration
- **🛡️ Security Headers:** OWASP security best practices implemented
