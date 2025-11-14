# Portfolio CMS - .NET Aspire Solution Structure

## Overview

This solution is built with .NET Aspire orchestration, providing a modern, cloud-native architecture with built-in observability, service discovery, and deployment capabilities.

## Solution Structure

```
PortfolioCMS.sln
├── PortfolioCMS.AppHost/          # .NET Aspire Orchestration
├── PortfolioCMS.ServiceDefaults/  # Shared Aspire Configurations
├── PortfolioCMS.API/              # ASP.NET Core Web API
├── PortfolioCMS.Frontend/         # Blazor Server Application
├── PortfolioCMS.DataAccess/       # Entity Framework Core Data Layer
└── Tests/                         # Test Projects (to be added)
```

## Technology Stack

- **.NET Version**: .NET 9.0 (will be upgraded to .NET 10 when released)
- **C# Version**: Preview (will be upgraded to C# 14 when available)
- **.NET Aspire**: 9.0.0 for orchestration and observability
- **Database**: Azure SQL Database with Entity Framework Core
- **Frontend**: Blazor Server
- **API**: ASP.NET Core Web API with Swagger/OpenAPI

## Projects

### PortfolioCMS.AppHost
The Aspire orchestration project that manages service discovery, deployment, and local development experience.

**Key Features:**
- Service discovery between Frontend and API
- SQL Server database orchestration
- Azure Key Vault integration
- Aspire Dashboard for monitoring

**Run the AppHost:**
```bash
dotnet run --project src/PortfolioCMS.AppHost
```

This will start:
- The Aspire Dashboard (http://localhost:15000)
- The API project
- The Frontend project
- SQL Server container (if using containerized database)

### PortfolioCMS.ServiceDefaults
Shared Aspire configurations for telemetry, health checks, and service discovery.

**Provides:**
- OpenTelemetry integration (metrics, traces, logs)
- Health check endpoints
- Service discovery configuration
- HTTP client resilience patterns

### PortfolioCMS.API
ASP.NET Core Web API backend providing RESTful services.

**Features:**
- JWT authentication
- Swagger/OpenAPI documentation
- Repository pattern with Unit of Work
- Aspire telemetry integration
- Health checks

**Endpoints:**
- Swagger UI: `/swagger`
- Health: `/health`
- API: `/api/v1/*`

### PortfolioCMS.Frontend
Blazor Server application for user interface.

**Features:**
- Server-side rendering
- Service discovery to API
- Aspire telemetry integration
- Responsive design

### PortfolioCMS.DataAccess
Entity Framework Core data access layer.

**Features:**
- Repository pattern
- Unit of Work pattern
- Entity models and relationships
- Database migrations
- SQL Server support

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Aspire workload: `dotnet workload install aspire`
- SQL Server (LocalDB, Docker, or Azure SQL Database)

### Running Locally

1. **Using Aspire AppHost (Recommended):**
   ```bash
   dotnet run --project src/PortfolioCMS.AppHost
   ```
   
   This starts all services with the Aspire Dashboard at http://localhost:15000

2. **Running Individual Projects:**
   ```bash
   # API
   dotnet run --project src/PortfolioCMS.API
   
   # Frontend
   dotnet run --project src/PortfolioCMS.Frontend
   ```

### Configuration

#### Connection Strings
Configure in `appsettings.json` or use Aspire's service discovery:

```json
{
  "ConnectionStrings": {
    "portfoliodb": "Server=(localdb)\\mssqllocaldb;Database=PortfolioCMS;Trusted_Connection=true;"
  }
}
```

#### Aspire Dashboard
The Aspire Dashboard provides:
- Real-time logs from all services
- Distributed tracing
- Metrics and performance data
- Service health status
- Resource management

## Aspire Benefits

### Service Discovery
The Frontend automatically discovers the API service endpoint through Aspire, eliminating hardcoded URLs:

```csharp
// Frontend automatically resolves "http://api" to the actual API endpoint
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("http://api");
});
```

### Observability
Built-in OpenTelemetry integration provides:
- Distributed tracing across services
- Metrics collection
- Structured logging
- Integration with Application Insights

### Resilience
Automatic retry policies, circuit breakers, and timeouts for HTTP calls.

### Deployment
Generate deployment manifests for Azure Container Apps:

```bash
azd init
azd up
```

## Development Workflow

1. **Start Aspire Dashboard**: `dotnet run --project src/PortfolioCMS.AppHost`
2. **View logs and traces** in the Aspire Dashboard
3. **Make changes** to API or Frontend
4. **Hot reload** automatically applies changes
5. **Monitor** service health and performance

## Project References

- **AppHost** → API, Frontend, ServiceDefaults
- **API** → DataAccess, ServiceDefaults
- **Frontend** → ServiceDefaults
- **DataAccess** → (no dependencies)

## Next Steps

- [ ] Upgrade to .NET 10 when released
- [ ] Upgrade to C# 14 when available
- [ ] Add test projects (PortfolioCMS.UnitTests, PortfolioCMS.IntegrationTests)
- [ ] Configure Azure deployment
- [ ] Set up CI/CD pipeline

## Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard)
- [Service Discovery](https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview)
- [Deployment](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/overview)
