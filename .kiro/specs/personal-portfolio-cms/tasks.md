# Implementation Plan

## Branch Strategy

When starting work on a major task (top-level numbered items like 1, 2, 3, etc.), create a feature branch following this naming convention:

- **Branch naming**: `feature/{task-number}-{short-description}`
- **Examples**:
  - Task 4 → `feature/4-frontend`
  - Task 5 → `feature/5-azure-infrastructure`
  - Task 6 → `feature/6-security`

**Workflow**:
1. Before starting a major task, create and checkout the feature branch from `main`
2. Complete all sub-tasks within that feature branch
3. Once all sub-tasks are complete and tested, create a pull request to merge back to `main`
4. After PR approval and merge, delete the feature branch

This approach keeps major features isolated and makes code review more manageable.

---

- [x] 1. Set up three-project solution structure and infrastructure foundation





  - [x] 1.1 Create solution structure with .NET Aspire orchestration





    - Create PortfolioCMS.sln targeting .NET 10 with C# 14
    - Add PortfolioCMS.Frontend project (Blazor Server, .NET 10, C# 14)
    - Add PortfolioCMS.API project (ASP.NET Core Web API, .NET 10, C# 14)
    - Add PortfolioCMS.DataAccess project (Class Library, .NET 10, C# 14)
    - Add PortfolioCMS.AppHost project (.NET Aspire orchestration, .NET 10)
    - Add PortfolioCMS.ServiceDefaults project (shared Aspire configurations, .NET 10)
    - Configure all projects to use C# 14 language features
    - Configure Aspire to orchestrate Frontend and API projects with service discovery
    - Set up proper project references and dependencies between projects
    - Configure solution-level NuGet packages and versioning for .NET 10
    - Note: Test projects (PortfolioCMS.UnitTests, PortfolioCMS.IntegrationTests) will be added in task 10.1
    - _Requirements: 8.1, 8.2, 8.3, 8.4_
  
  - [x] 1.2 Initialize infrastructure as code foundation


    - Create infrastructure/ directory with OpenTofu/Terraform configuration
    - Set up main.tf, variables.tf, and outputs.tf for Azure resources
    - Configure Terraform backend for state management in Azure Storage
    - _Requirements: 9.2, 9.5_
  
  - [x] 1.3 Set up CI/CD pipeline foundation


    - Create .github/workflows/ directory with basic CI pipeline
    - Configure GitHub Actions for automated builds and tests
    - Set up SonarCloud and OWASP ZAP integration placeholders
    - _Requirements: 9.1, 9.3, 10.1, 10.2_

- [x] 2. Implement DataAccess project with Azure SQL Database





  - [x] 2.1 Create core entity models for Azure SQL Database


    - Implement Article, Comment, Tag, Project, MediaFile, and AspNetUser entities
    - Configure entity relationships and navigation properties for Azure SQL
    - Add data annotations optimized for Azure SQL Database constraints
    - _Requirements: 7.1, 7.2, 8.3_
  
  - [x] 2.2 Configure Entity Framework Core 10 DbContext for Azure SQL





    - Create ApplicationDbContext using EF Core 10 with C# 14 features
    - Configure entity relationships using Fluent API with proper indexing
    - Leverage C# 14 primary constructors and collection expressions where applicable
    - Set up connection string management with Azure Key Vault integration using Managed Identity
    - Configure for Azure SQL Database Free tier optimizations
    - _Requirements: 7.1, 7.2, 7.6, 12.4, 12.6_
  


  - [x] 2.3 Implement repository pattern and Unit of Work
    - Create IRepository<T> interface and base repository implementation
    - Implement specific repositories (IArticleRepository, ICommentRepository, etc.)
    - Create IUnitOfWork interface and implementation for transaction management
    - _Requirements: 7.4, 8.3_
  

  - [x] 2.4 Create and apply Azure SQL Database migrations






    - Generate initial migration for all entities with Azure SQL Free tier optimizations
    - Create comprehensive seed data for initial admin user and sample content (within 32 MB limit)
    - Apply migrations and verify database schema in Azure SQL Database Free tier
    - _Requirements: 7.2, 7.3_

- [x] 3. Build API project with comprehensive observability





  - [x] 3.1 Set up API project foundation with Aspire integration





    - Create ASP.NET Core Web API project with .NET Aspire service defaults
    - Configure Swagger/OpenAPI documentation with comprehensive schemas
    - Set up dependency injection container with service registrations
    - Add Aspire telemetry, health checks, and service discovery
    - Configure Aspire dashboard integration for local development
    - _Requirements: 6.1, 6.2, 6.3, 8.4, 11.2_
  
  - [x] 3.2 Implement authentication and authorization system


    - Configure ASP.NET Core Identity with Azure Active Directory integration
    - Implement JWT token generation and validation with secrets from Azure Key Vault
    - Create role-based authorization policies (Admin, Viewer)
    - Configure Managed Identity for secure Key Vault access
    - _Requirements: 3.1, 7.6, 12.4, 12.6_
  


  - [x] 3.3 Create business logic services with observability





    - Implement ContentService, CommentService, MediaService with telemetry
    - Add ObservabilityService for Application Insights and Grafana Cloud integration
    - Create service interfaces and implementations with proper error handling


    - _Requirements: 2.1, 2.3, 3.2, 3.5, 4.1, 11.1, 11.3_
  
  - [x] 3.4 Build API controllers with comprehensive documentation






    - Create ArticlesController, ProjectsController, CommentsController with OpenAPI docs
    - Implement AdminController for content management operations
    - Add HealthController for monitoring and readiness checks
    - _Requirements: 1.1, 2.1, 2.2, 3.2, 5.1, 6.4, 11.4_

- [x] 4. Develop Frontend project with Blazor Server







  - [x] 4.1 Set up Blazor Server project with Aspire integration


    - Create PortfolioCMS.Frontend project with Blazor Server components
    - Add Aspire service defaults for telemetry and health checks
    - Configure service discovery to communicate with API project via Aspire
    - Set up authentication integration with API project using Aspire service references
    - Configure Aspire dashboard integration for monitoring
    - _Requirements: 1.1, 8.1, 8.4, 11.2_
  
  - [x] 4.2 Create public-facing pages and components


    - Implement Home/Resume page displaying portfolio sections with responsive design
    - Create Blog listing page with search, pagination, and SEO optimization
    - Build Article detail page with comment display and submission functionality
    - Add Contact page with message form and validation
    - _Requirements: 1.1, 1.2, 1.3, 2.1, 2.2, 2.4, 5.1, 5.4_
  
  - [x] 4.3 Build admin dashboard with rich functionality


    - Create admin layout with navigation and role-based authentication
    - Implement article management pages (list, create, edit) with rich text editor
    - Build comment moderation interface with bulk operations
    - Add media management page for Azure Blob Storage file uploads
    - _Requirements: 3.1, 3.2, 3.3, 3.5, 4.4_
  
  - [x] 4.4 Implement API integration services with Aspire service discovery


    - Create ApiService for HTTP communication with API project using Aspire service discovery
    - Configure HttpClient with Aspire-provided service endpoints
    - Add error handling, retry logic, and resilience patterns with Polly
    - Implement caching strategies for frequently accessed data
    - Leverage Aspire telemetry for tracking API calls
    - _Requirements: 8.4, 11.3_

- [x] 5. Implement Azure infrastructure with OpenTofu/Terraform




  - [x] 5.1 Create core Azure infrastructure modules


    - Implement Azure Resource Group, App Service Plan, and App Service modules with system-assigned managed identity
    - Create Azure SQL Database Free tier and SQL Server modules (32 MB storage limit)
    - Add Azure Key Vault module for secrets management with managed identity access policies
    - Configure Key Vault to store database passwords and connection strings
    - _Requirements: 7.6, 9.2, 12.1, 12.2, 12.4, 12.6_
  


  - [x] 5.2 Set up Azure networking and security infrastructure

    - Implement Azure Application Gateway module with WAF protection
    - Create Azure CDN module for static asset delivery
    - Add Azure Storage Account module for media files and Terraform state

    - _Requirements: 12.3, 12.5_
  
  - [x] 5.3 Configure monitoring and observability infrastructure

    - Create Azure Application Insights module with custom dashboards
    - Set up Azure Monitor and Log Analytics workspace
    - Configure Grafana Cloud integration and alerting rules
    - _Requirements: 11.1, 11.2, 11.4, 11.5_
  

  - [x] 5.4 Implement environment-specific configurations

    - Create development (Free tier), staging (Basic tier), and production (Standard tier) environment configurations
    - Set up Terraform workspaces and variable management for different SQL tiers
    - Configure automated infrastructure deployment in CI/CD pipeline
    - Document manual Azure CLI steps for Key Vault secret initialization
    - _Requirements: 7.6, 9.4, 9.5, 12.6_

- [x] 6. Implement comprehensive security measures




  - [x] 6.1 Configure multi-layer security in API project


    - Implement security headers middleware with CSP, HSTS, and XSS protection
    - Add input validation and sanitization for all API endpoints
    - Configure CORS policies for cross-origin requests
    - _Requirements: 10.3, 10.4_
  


  - [ ] 6.2 Set up Azure security services integration
    - Integrate Azure Key Vault for secrets and certificate management using Managed Identity
    - Store all sensitive credentials (database passwords, connection strings, API keys) in Key Vault
    - Configure Azure Active Directory authentication and authorization
    - Implement Azure Application Gateway WAF rules and policies


    - _Requirements: 7.6, 12.4, 12.5, 12.6_
  
  - [ ] 6.3 Add security scanning and compliance
    - Configure SonarCloud static code analysis with security rules


    - Set up OWASP ZAP dynamic security scanning in CI/CD pipeline
    - Implement dependency vulnerability scanning with Snyk or similar
    - _Requirements: 10.1, 10.2, 10.4_
  
  - [ ] 6.4 Create security testing and validation
    - Write security-focused unit tests for authentication and authorization
    - Implement integration tests for security headers and CORS policies
    - Add penetration testing scripts for common vulnerabilities
    - _Requirements: 10.1, 10.2_

- [ ] 7. Build comprehensive CI/CD pipeline with GitHub Actions
  - [ ] 7.1 Create automated build and test pipeline
    - Implement GitHub Actions workflow for automated builds of all projects (API, Frontend, DataAccess)
    - Add separate unit test execution with code coverage reporting for PortfolioCMS.UnitTests
    - Configure integration test execution with Azure SQL Database for PortfolioCMS.IntegrationTests
    - Set up parallel test execution and test result aggregation
    - Configure test failure notifications and coverage thresholds
    - _Requirements: 9.1, 9.3_
  
  - [ ] 7.2 Set up security and quality gates
    - Integrate SonarCloud static code analysis with quality gates
    - Add OWASP ZAP security scanning for deployed applications
    - Configure dependency vulnerability scanning and reporting
    - _Requirements: 10.1, 10.2, 10.4_
  
  - [ ] 7.3 Implement infrastructure deployment automation
    - Create GitHub Actions workflow for Terraform infrastructure deployment
    - Add environment-specific deployment pipelines (dev, staging, production)
    - Configure Azure service principal authentication for deployments
    - _Requirements: 9.2, 9.4, 9.5_
  
  - [ ] 7.4 Set up application deployment pipeline with Aspire
    - Configure Aspire deployment manifest generation for Azure Container Apps
    - Implement Docker containerization for Frontend and API projects
    - Create Azure Container Registry integration for image storage
    - Configure Aspire to deploy to Azure Container Apps with automatic service discovery
    - Add blue-green deployment strategy for zero-downtime deployments
    - Set up Aspire dashboard for production monitoring
    - _Requirements: 9.1, 12.1_

- [ ] 8. Implement comprehensive observability and monitoring with Aspire
  - [ ] 8.1 Set up Aspire telemetry and Application Insights integration
    - Configure Aspire service defaults with OpenTelemetry for distributed tracing
    - Add Application Insights integration through Aspire for both Frontend and API
    - Implement custom metrics and events using Aspire telemetry APIs
    - Configure Aspire dashboard for local development monitoring
    - Set up structured logging with Aspire's built-in logging providers
    - _Requirements: 8.4, 11.2, 11.3_
  
  - [ ] 8.2 Create Grafana Cloud dashboards and alerting
    - Set up Grafana Cloud integration with custom dashboards
    - Create alerting rules for critical system metrics and business KPIs
    - Implement notification channels for different alert severities
    - _Requirements: 11.1, 11.5_
  
  - [ ] 8.3 Add comprehensive logging and monitoring with Aspire
    - Implement structured logging using Aspire's built-in logging with OpenTelemetry
    - Set up Azure Monitor Logs for centralized log aggregation via Aspire
    - Configure Aspire dashboard for real-time log viewing and filtering
    - Create custom queries and alerts for error patterns and performance issues
    - Leverage Aspire's distributed tracing for cross-service debugging
    - _Requirements: 8.4, 11.4_
  
  - [ ] 8.4 Create monitoring and alerting tests
    - Write tests for custom telemetry and metrics collection
    - Implement synthetic monitoring tests for critical user journeys
    - Add performance benchmarking tests for API endpoints
    - _Requirements: 11.3, 11.4_

- [ ] 9. Add advanced functionality and performance optimization
  - [ ] 9.1 Implement search and filtering capabilities
    - Add full-text search across article titles and content using Azure SQL Database features
    - Implement tag-based filtering and faceted search for articles
    - Create search results page with highlighting and pagination
    - _Requirements: 2.5_
  
  - [ ] 9.2 Configure caching and performance optimization
    - Implement multi-tier caching strategy with in-memory and distributed caching
    - Add Azure CDN integration for static assets and media files
    - Configure database query optimization and connection pooling
    - _Requirements: 12.3_
  
  - [ ] 9.3 Add media management with Azure Blob Storage
    - Implement Azure Blob Storage integration for media file storage
    - Add image processing and optimization capabilities
    - Create media library with search and organization features
    - _Requirements: 4.1, 4.2, 4.3, 4.4_
  
  - [ ] 9.4 Implement performance testing and optimization
    - Create load testing scenarios with NBomber or Azure Load Testing
    - Add performance monitoring and alerting for response times
    - Implement database query performance analysis and optimization
    - _Requirements: 12.1, 12.2_

- [ ] 10. Set up comprehensive testing infrastructure with separate projects
  - [ ] 10.1 Create dedicated test project structure
    - Create PortfolioCMS.UnitTests project for isolated unit testing
    - Create PortfolioCMS.IntegrationTests project for integration testing
    - Set up proper project references and test dependencies
    - Configure test runners, coverage tools, and reporting
    - _Requirements: 7.4, 8.3, 9.1, 9.3_
  
  - [ ] 10.2 Implement comprehensive unit tests for DataAccess project
    - Create unit tests for repository implementations using in-memory database
    - Add tests for entity models, relationships, and validation rules
    - Implement tests for DbContext configuration and Entity Framework mappings
    - Test Unit of Work pattern and transaction management
    - Add tests for database migrations and seed data
    - _Requirements: 7.1, 7.2, 7.4, 8.3_
  
  - [ ] 10.3 Build comprehensive unit tests for API project
    - Create unit tests for business services (ContentService, CommentService, MediaService) with mocked dependencies
    - Add unit tests for API controllers with mocked services and proper HTTP response testing
    - Implement tests for authentication, authorization, and JWT token handling
    - Test middleware components, validation logic, and error handling
    - Add tests for ObservabilityService and telemetry collection
    - _Requirements: 2.1, 2.3, 3.1, 3.2, 6.4, 11.1, 11.3_
  
  - [ ] 10.4 Develop unit tests for Frontend project
    - Create unit tests for Blazor components using bUnit framework
    - Add tests for API integration services and HTTP client error handling
    - Implement tests for authentication flows and role-based UI components
    - Test form validation, data binding, and component lifecycle
    - Add tests for caching strategies and state management
    - _Requirements: 1.1, 8.1, 8.4_
  
  - [ ] 10.5 Create comprehensive integration tests
    - Implement API integration tests using TestServer with real database connections
    - Add database integration tests with Azure SQL Database test instances
    - Create end-to-end workflow tests covering complete user journeys
    - Test cross-project integration between API and Frontend
    - Implement tests for external service integrations (Azure services, authentication)
    - Add performance and load testing scenarios
    - _Requirements: All requirements (integration testing)_
  
  - [ ] 10.6 Set up advanced testing scenarios
    - Create security integration tests for authentication and authorization flows
    - Implement tests for Azure Blob Storage media file operations
    - Add tests for search functionality and database query performance
    - Create tests for caching mechanisms and cache invalidation
    - Implement chaos engineering tests for resilience validation
    - Add contract tests for API versioning and backward compatibility
    - _Requirements: 2.5, 4.1, 4.2, 10.3, 10.4, 12.3_

- [ ] 11. Implement validation, error handling, and resilience
  - [ ] 11.1 Add comprehensive validation across all projects
    - Implement FluentValidation in API project with custom business rules
    - Add client-side validation in Blazor forms with real-time feedback
    - Create validation for file uploads, content sanitization, and security
    - _Requirements: 2.3, 4.2, 5.3, 10.3_
  
  - [ ] 11.2 Configure global error handling and resilience
    - Implement global exception middleware in API project with observability
    - Add error boundaries and fallback UI in Blazor components
    - Create retry policies and circuit breakers for external dependencies
    - _Requirements: All requirements (error handling)_
  
  - [ ] 11.3 Add rate limiting and abuse protection
    - Implement rate limiting for public API endpoints
    - Add CAPTCHA integration for comment submission and contact forms
    - Create IP-based blocking and suspicious activity detection
    - _Requirements: 2.3, 5.3, 10.4_
  
  - [ ] 11.4 Create resilience and disaster recovery testing
    - Implement chaos engineering tests for system resilience
    - Add disaster recovery testing scenarios
    - Create automated backup and restore validation tests
    - _Requirements: 7.5, 12.2_

- [ ] 12. Final integration, deployment, and production readiness
  - [ ] 12.1 Complete Aspire-orchestrated integration and testing
    - Wire all projects together using Aspire AppHost orchestration
    - Verify service discovery and communication between Frontend and API via Aspire
    - Test admin and public user workflows across all projects end-to-end
    - Validate Aspire dashboard shows correct telemetry and health status
    - Test local development experience with Aspire's hot reload and debugging
    - _Requirements: 8.4, 8.5, All requirements_
  
  - [ ] 12.2 Deploy to Azure with Aspire and full infrastructure
    - Execute manual Azure CLI steps to initialize Key Vault secrets for database password
    - Execute Terraform deployment to create Azure Container Apps infrastructure
    - Generate Aspire deployment manifest for Azure Container Apps
    - Deploy Frontend and API projects to Azure Container Apps using Aspire
    - Configure Azure SQL Database Free tier with automated backups (7-day retention)
    - Verify Aspire service discovery works in Azure Container Apps environment
    - Verify Key Vault integration and secret retrieval using managed identity
    - Configure Aspire dashboard for production monitoring
    - _Requirements: 7.6, 8.4, 9.5, 12.1, 12.2, 12.5, 12.6_
  
  - [ ] 12.3 Configure production monitoring and alerting
    - Activate all Grafana Cloud dashboards and alerting rules
    - Verify Azure Application Insights telemetry and custom metrics
    - Test incident response procedures and notification channels
    - _Requirements: 11.1, 11.4, 11.5_
  
  - [ ] 12.4 Validate security and compliance in production
    - Execute final security scans with OWASP ZAP against production environment
    - Verify all Azure security services are properly configured
    - Conduct security review and penetration testing validation
    - _Requirements: 10.1, 10.2, 10.4, 12.4, 12.5_
  
  - [ ] 12.5 Create production documentation and runbooks
    - Create operational runbooks for common maintenance tasks
    - Document disaster recovery procedures and backup strategies
    - Prepare user documentation and admin guides
    - _Requirements: 7.5, 9.5_