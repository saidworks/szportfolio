# CI/CD Pipeline Documentation

This directory contains GitHub Actions workflows for the Portfolio CMS project, implementing a comprehensive CI/CD pipeline with security scanning and infrastructure as code.

## Workflow Overview

### 1. CI Pipeline (`ci.yml`)
**Trigger**: Push to `main`/`develop` branches, Pull Requests
**Purpose**: Continuous Integration with build, test, and quality checks

**Jobs**:
- **Build and Test**: Compile solution, run unit tests, generate code coverage
- **Security Scan**: Static code analysis and dependency vulnerability scanning
- **Infrastructure Validation**: Terraform format, validation, and plan checks
- **Code Quality**: Code analysis and metrics generation
- **Notification**: Status reporting and alerts

### 2. Staging Deployment (`deploy-staging.yml`)
**Trigger**: Push to `develop` branch, Manual dispatch
**Purpose**: Automated deployment to staging environment

**Jobs**:
- **Pre-deployment Checks**: Build, test, and security validation
- **Deploy Infrastructure**: Terraform deployment to staging
- **Build and Publish**: Create deployment artifacts
- **Deploy Applications**: Deploy to Azure App Services
- **Database Migrations**: Run EF Core migrations
- **Post-deployment Tests**: Health checks and integration tests
- **Notification**: Deployment status reporting

### 3. Production Deployment (`deploy-production.yml`)
**Trigger**: Manual dispatch only (with confirmation)
**Purpose**: Controlled production deployment with blue-green strategy

**Jobs**:
- **Validate Deployment**: Confirmation and branch validation
- **Pre-production Checks**: Comprehensive testing and security scans
- **Backup Production**: Database and configuration backups
- **Deploy Infrastructure**: Production infrastructure deployment
- **Build Artifacts**: Production-optimized builds
- **Blue-Green Deployment**: Zero-downtime deployment strategy
- **Database Migrations**: Production database updates
- **Post-production Validation**: Health checks and performance validation
- **Rollback Plan**: Automated rollback on failure

### 4. SonarCloud Analysis (`sonarcloud.yml`)
**Trigger**: Push to `main`/`develop`, Pull Requests
**Purpose**: Comprehensive static code analysis and quality gates

**Features**:
- Code quality analysis
- Security vulnerability detection
- Code coverage reporting
- Technical debt assessment
- Quality gate enforcement

### 5. Security Scanning (`security-scan.yml`)
**Trigger**: Daily schedule, Manual dispatch
**Purpose**: Comprehensive security testing

**Scans**:
- **Dependency Vulnerabilities**: Third-party package security
- **Static Code Analysis**: Security-focused code review
- **OWASP ZAP**: Dynamic application security testing
- **Container Security**: Docker image vulnerability scanning
- **SSL/TLS Configuration**: Certificate and encryption validation
- **Security Headers**: HTTP security header verification

## Configuration Requirements

### GitHub Secrets

Configure the following secrets in your GitHub repository:

#### Azure Authentication
```
ARM_CLIENT_ID          # Azure Service Principal Client ID
ARM_CLIENT_SECRET      # Azure Service Principal Client Secret
ARM_SUBSCRIPTION_ID    # Azure Subscription ID
ARM_TENANT_ID          # Azure Tenant ID
```

#### Database Configuration
```
TF_VAR_sql_admin_password    # SQL Server admin password
```

#### Security Scanning
```
SONAR_TOKEN           # SonarCloud authentication token
SNYK_TOKEN           # Snyk authentication token (optional)
```

### Environment Configuration

#### GitHub Environments
Create the following environments in GitHub repository settings:
- `production` - Requires manual approval for production deployments

#### Branch Protection Rules
Configure branch protection for:
- `main` - Require PR reviews, status checks, up-to-date branches
- `develop` - Require status checks

### Azure Service Principal Setup

Create a service principal for GitHub Actions:

```bash
# Create service principal
az ad sp create-for-rbac --name "github-actions-portfoliocms" \
  --role contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth

# Assign additional roles if needed
az role assignment create \
  --assignee {client-id} \
  --role "Key Vault Contributor" \
  --scope /subscriptions/{subscription-id}
```

## Workflow Triggers

### Automatic Triggers
- **CI Pipeline**: Every push and PR
- **Staging Deployment**: Push to `develop` branch
- **Security Scanning**: Daily at 2 AM UTC
- **SonarCloud Analysis**: Push and PR events

### Manual Triggers
- **Production Deployment**: Manual dispatch with confirmation
- **Security Scanning**: On-demand with environment selection
- **Staging Deployment**: Manual override option

## Security Features

### Static Analysis
- SonarCloud integration with security rules
- Dependency vulnerability scanning
- Code quality gates

### Dynamic Testing
- OWASP ZAP security scanning
- SSL/TLS configuration validation
- Security headers verification

### Infrastructure Security
- Terraform security validation
- Azure security best practices
- Secrets management with Key Vault

## Deployment Strategy

### Staging Environment
- Automatic deployment from `develop` branch
- Integration testing and security scanning
- Performance validation

### Production Environment
- Manual approval required
- Blue-green deployment strategy
- Comprehensive backup procedures
- Automated rollback on failure

## Monitoring and Alerting

### Build Status
- GitHub status checks
- Email notifications on failure
- Slack/Teams integration (configurable)

### Security Alerts
- Vulnerability notifications
- Security scan reports
- Compliance status updates

### Deployment Notifications
- Deployment success/failure alerts
- Performance metrics reporting
- Health check status

## Troubleshooting

### Common Issues

#### Build Failures
1. Check .NET version compatibility
2. Verify NuGet package restoration
3. Review test failures and code coverage

#### Deployment Issues
1. Verify Azure credentials and permissions
2. Check Terraform state and plan output
3. Validate Azure resource availability

#### Security Scan Failures
1. Review vulnerability reports
2. Check security tool configuration
3. Validate target environment accessibility

### Useful Commands

```bash
# Local testing
dotnet build ./PortfolioCMS.sln
dotnet test ./PortfolioCMS.sln
terraform plan -var-file="environments/development/terraform.tfvars"

# GitHub CLI workflow management
gh workflow list
gh workflow run ci.yml
gh workflow view deploy-production.yml
```

## Best Practices

### Code Quality
- Maintain >80% code coverage
- Follow SonarCloud quality gates
- Regular dependency updates

### Security
- Regular security scanning
- Prompt vulnerability remediation
- Security-first development practices

### Deployment
- Test in staging before production
- Use feature flags for gradual rollouts
- Maintain deployment documentation

## Future Enhancements

### Planned Features
- Container-based deployments
- Multi-region deployment support
- Advanced monitoring and alerting
- Performance testing integration
- Automated security remediation

### Integration Opportunities
- Azure DevOps integration
- Grafana Cloud dashboards
- Advanced security scanning tools
- Performance monitoring tools

## Support and Maintenance

### Regular Tasks
- Update workflow dependencies
- Review and update security configurations
- Monitor pipeline performance
- Update documentation

### Incident Response
1. Check workflow run logs
2. Review Azure resource status
3. Validate security scan results
4. Execute rollback procedures if needed

For detailed troubleshooting and support, refer to the main project documentation and Azure/GitHub documentation.