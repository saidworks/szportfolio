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

## ☁️ Azure Deployment

### Prerequisites for Azure Deployment

1. **Azure CLI** - [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **OpenTofu/Terraform** - [Install OpenTofu](https://opentofu.org/docs/intro/install/)
3. **Active Azure Subscription** with appropriate permissions

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
- **🛡️ Security Headers:** OWASP security best practices implemented
