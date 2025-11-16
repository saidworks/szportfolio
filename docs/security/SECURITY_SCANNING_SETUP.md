# Security Scanning Setup Guide

This guide provides step-by-step instructions for setting up all security scanning tools in the Portfolio CMS project.

## Prerequisites

- GitHub repository with admin access
- Azure subscription (for deployed environments)
- Access to create accounts on third-party services

## Quick Start

The security scanning infrastructure is already configured in the repository. To activate it, you need to:

1. Configure SonarCloud (5 minutes)
2. Configure Snyk (3 minutes)
3. Verify GitHub CodeQL is enabled (1 minute)
4. Deploy application for ZAP scanning (varies)

## Detailed Setup Instructions

### 1. SonarCloud Setup

SonarCloud provides static code analysis with security rules.

#### Step 1: Create SonarCloud Account

1. Go to [https://sonarcloud.io](https://sonarcloud.io)
2. Click "Log in" and select "With GitHub"
3. Authorize SonarCloud to access your GitHub account

#### Step 2: Create Organization

1. Click the "+" icon in the top right
2. Select "Create new organization"
3. Choose "Free plan" for open source projects
4. Enter organization key (e.g., `your-username` or `your-org-name`)
5. Click "Create Organization"

#### Step 3: Import Repository

1. Click "Analyze new project"
2. Select your Portfolio CMS repository
3. Click "Set Up"
4. Choose "With GitHub Actions" as the analysis method

#### Step 4: Generate Token

1. Go to My Account > Security
2. Enter a token name (e.g., `PortfolioCMS-GitHub-Actions`)
3. Click "Generate"
4. Copy the token (you won't see it again)

#### Step 5: Configure GitHub Secret

1. Go to your GitHub repository
2. Navigate to Settings > Secrets and variables > Actions
3. Click "New repository secret"
4. Name: `SONAR_TOKEN`
5. Value: Paste the token from SonarCloud
6. Click "Add secret"

#### Step 6: Update Workflow

1. Open `.github/workflows/sonarcloud.yml`
2. Find the line with `/o:"your-organization"`
3. Replace `your-organization` with your SonarCloud organization key
4. Commit and push the change

#### Step 7: Verify Setup

1. Push a commit to trigger the workflow
2. Go to Actions tab in GitHub
3. Check that "SonarCloud Analysis" workflow runs successfully
4. Visit SonarCloud dashboard to see results

**Estimated Time**: 5-10 minutes

---

### 2. Snyk Setup

Snyk provides dependency vulnerability scanning.

#### Step 1: Create Snyk Account

1. Go to [https://snyk.io](https://snyk.io)
2. Click "Sign up" and select "Sign up with GitHub"
3. Authorize Snyk to access your GitHub account

#### Step 2: Import Repository

1. Click "Add project"
2. Select "GitHub"
3. Find and select your Portfolio CMS repository
4. Click "Add selected repositories"

#### Step 3: Generate API Token

1. Click on your profile icon (top right)
2. Select "Account settings"
3. Navigate to "General" section
4. Find "Auth Token" section
5. Click "Show" and copy the token

#### Step 4: Configure GitHub Secret

1. Go to your GitHub repository
2. Navigate to Settings > Secrets and variables > Actions
3. Click "New repository secret"
4. Name: `SNYK_TOKEN`
5. Value: Paste the token from Snyk
6. Click "Add secret"

#### Step 5: Verify Setup

1. Push a commit to trigger the CI workflow
2. Go to Actions tab in GitHub
3. Check that Snyk scan runs in the "Security Scan" job
4. Visit Snyk dashboard to see vulnerability reports

**Estimated Time**: 3-5 minutes

---

### 3. GitHub CodeQL Setup

CodeQL is GitHub's built-in code scanning tool.

#### Step 1: Enable Code Scanning

1. Go to your GitHub repository
2. Navigate to Settings > Code security and analysis
3. Find "Code scanning" section
4. Click "Set up" next to "CodeQL analysis"
5. Select "Advanced" setup

#### Step 2: Verify Configuration

CodeQL is already configured in `.github/workflows/ci.yml` and `.github/workflows/security-scan.yml`.

1. Go to Actions tab
2. Verify that CodeQL analysis runs on push/PR
3. Check Security tab > Code scanning for results

#### Step 3: Configure Alerts

1. Go to Settings > Code security and analysis
2. Enable "Dependency graph"
3. Enable "Dependabot alerts"
4. Enable "Dependabot security updates"

**Estimated Time**: 1-2 minutes

---

### 4. OWASP ZAP Setup

OWASP ZAP performs dynamic security testing on deployed applications.

#### Prerequisites

- Application deployed to staging or production
- Public URL accessible for scanning

#### Step 1: Update Target URLs

1. Open `.github/workflows/security-scan.yml`
2. Find the `env` section at the top
3. Update the following URLs:
   ```yaml
   STAGING_FRONTEND_URL: 'https://your-staging-frontend.azurewebsites.net'
   STAGING_API_URL: 'https://your-staging-api.azurewebsites.net'
   PRODUCTION_FRONTEND_URL: 'https://your-production-frontend.azurewebsites.net'
   PRODUCTION_API_URL: 'https://your-production-api.azurewebsites.net'
   ```
4. Commit and push changes

#### Step 2: Configure ZAP Rules

The ZAP rules are already configured in `.zap/rules.tsv`. Review and adjust as needed:

1. Open `.zap/rules.tsv`
2. Review the rules and thresholds
3. Adjust based on your security requirements
4. Commit any changes

#### Step 3: Run Manual Scan

1. Go to Actions tab in GitHub
2. Select "Security Scanning" workflow
3. Click "Run workflow"
4. Select target environment (staging/production)
5. Select scan type (quick/full/baseline)
6. Click "Run workflow"

#### Step 4: Review Results

1. Wait for workflow to complete
2. Check workflow artifacts for detailed reports
3. Review any issues created in the Issues tab
4. Address high-priority findings

**Estimated Time**: 5 minutes setup + scan time (varies)

---

### 5. OWASP Dependency Check Setup

OWASP Dependency Check is pre-configured and requires no additional setup.

#### Verify Configuration

1. Check `.github/workflows/security-scan.yml`
2. Verify OWASP Dependency Check job exists
3. Review `.github/dependency-check-suppressions.xml` for suppressions

#### Customize Suppressions

1. Open `.github/dependency-check-suppressions.xml`
2. Add suppressions for false positives
3. Document reason for each suppression
4. Commit changes

**Estimated Time**: Already configured

---

### 6. Trivy Container Scanning Setup

Trivy is pre-configured for container scanning.

#### Prerequisites

- Docker containers built in CI/CD pipeline
- Container images available for scanning

#### Verify Configuration

1. Check `.github/workflows/security-scan.yml`
2. Verify Trivy scanning job exists
3. Ensure it runs when containers are built

**Estimated Time**: Already configured

---

### 7. SSL/TLS Testing Setup

SSL/TLS testing with testssl.sh is pre-configured.

#### Verify Configuration

1. Check `.github/workflows/security-scan.yml`
2. Verify SSL/TLS scan job exists
3. Ensure target URLs are correct

**Estimated Time**: Already configured

---

## Verification Checklist

After completing the setup, verify that all scans are working:

- [ ] SonarCloud analysis runs on push/PR
- [ ] SonarCloud dashboard shows project data
- [ ] Snyk scan runs in CI pipeline
- [ ] Snyk dashboard shows vulnerabilities
- [ ] CodeQL analysis runs on push/PR
- [ ] Security tab shows code scanning alerts
- [ ] OWASP ZAP can be triggered manually
- [ ] OWASP Dependency Check runs in security scan
- [ ] Trivy scans containers (when built)
- [ ] SSL/TLS tests run on schedule
- [ ] Security headers validation runs

## Scheduled Scans

The following scans run automatically:

| Scan | Schedule | Workflow |
|------|----------|----------|
| SonarCloud | On push/PR | `sonarcloud.yml` |
| Snyk | On push/PR + Daily | `ci.yml`, `security-scan.yml` |
| CodeQL | On push/PR | `ci.yml`, `security-scan.yml` |
| OWASP Dependency Check | Daily 2 AM UTC | `security-scan.yml` |
| OWASP ZAP | Weekly Sunday 2 AM UTC | `security-scan-zap.yml` |
| SSL/TLS | Daily 2 AM UTC | `security-scan.yml` |
| Security Headers | Daily 2 AM UTC | `security-scan.yml` |

## GitHub Secrets Required

Ensure the following secrets are configured in GitHub:

| Secret Name | Required | Purpose | Setup Guide |
|-------------|----------|---------|-------------|
| `SONAR_TOKEN` | Yes | SonarCloud authentication | Section 1 |
| `SNYK_TOKEN` | Yes | Snyk authentication | Section 2 |
| `ARM_CLIENT_ID` | Optional | Azure Terraform auth | Infrastructure docs |
| `ARM_CLIENT_SECRET` | Optional | Azure Terraform auth | Infrastructure docs |
| `ARM_SUBSCRIPTION_ID` | Optional | Azure Terraform auth | Infrastructure docs |
| `ARM_TENANT_ID` | Optional | Azure Terraform auth | Infrastructure docs |

## Troubleshooting

### SonarCloud Issues

**Problem**: SonarCloud scan fails with authentication error

**Solution**:
1. Verify `SONAR_TOKEN` is set in GitHub secrets
2. Check token hasn't expired in SonarCloud
3. Regenerate token if needed

**Problem**: Quality gate fails

**Solution**:
1. Review SonarCloud dashboard for specific issues
2. Fix code quality or security issues
3. Adjust quality gate settings if needed

### Snyk Issues

**Problem**: Snyk scan fails with rate limit error

**Solution**:
1. Upgrade to paid Snyk plan for higher limits
2. Reduce scan frequency
3. Use Snyk CLI locally for development

**Problem**: Too many vulnerability alerts

**Solution**:
1. Update dependencies to latest versions
2. Review and suppress false positives
3. Create remediation plan for true positives

### ZAP Issues

**Problem**: ZAP scan times out

**Solution**:
1. Increase timeout in workflow
2. Use baseline scan instead of full scan
3. Optimize application response time

**Problem**: Too many false positives

**Solution**:
1. Review and update `.zap/rules.tsv`
2. Add specific rules to ignore false positives
3. Use ZAP context files for authenticated scanning

### General Issues

**Problem**: Workflow fails with permission error

**Solution**:
1. Check GitHub Actions permissions in repository settings
2. Ensure workflows have write permissions for Security tab
3. Verify secrets are accessible to workflows

**Problem**: Scans take too long

**Solution**:
1. Run expensive scans on schedule instead of every push
2. Use caching for dependencies
3. Parallelize independent scan jobs

## Best Practices

1. **Start with SonarCloud and Snyk**: These provide the most immediate value
2. **Review results regularly**: Don't let security alerts pile up
3. **Fix issues incrementally**: Address high-priority issues first
4. **Update suppressions**: Review and remove outdated suppressions
5. **Monitor scan performance**: Ensure scans don't slow down development
6. **Train the team**: Ensure developers understand security scan results
7. **Integrate with workflow**: Make security scanning part of code review

## Next Steps

After completing the setup:

1. Review initial scan results
2. Create remediation plan for findings
3. Set up notifications for security alerts
4. Schedule regular security review meetings
5. Document security policies and procedures
6. Train development team on security tools
7. Integrate security scanning into code review process

## Support

For help with security scanning setup:

- **SonarCloud**: [SonarCloud Documentation](https://docs.sonarcloud.io/)
- **Snyk**: [Snyk Documentation](https://docs.snyk.io/)
- **GitHub Security**: [GitHub Security Documentation](https://docs.github.com/en/code-security)
- **OWASP ZAP**: [ZAP Documentation](https://www.zaproxy.org/docs/)

---

**Last Updated**: 2024-01-15
**Review Schedule**: Quarterly
**Document Owner**: Security Team
