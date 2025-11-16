# Security Scanning and Compliance

This document describes the comprehensive security scanning and compliance measures implemented in the Portfolio CMS project.

## Overview

The project implements multiple layers of security scanning to identify vulnerabilities, enforce code quality, and maintain compliance with security best practices. All security scans are integrated into the CI/CD pipeline and run automatically.

## Security Scanning Tools

### 1. SonarCloud - Static Code Analysis

**Purpose**: Detect code quality issues, security vulnerabilities, and code smells in the source code.

**Configuration**: `.github/workflows/sonarcloud.yml` and `sonar-project.properties`

**Features**:
- Static code analysis with security-focused rules
- Code coverage tracking
- Technical debt measurement
- Security hotspot detection
- Quality gate enforcement

**Setup Instructions**:

1. Go to [SonarCloud](https://sonarcloud.io) and sign in with GitHub
2. Create a new organization or use an existing one
3. Import this repository as a new project
4. Generate a token in My Account > Security
5. Add `SONAR_TOKEN` to GitHub repository secrets
6. Update the organization key in `.github/workflows/sonarcloud.yml`

**Quality Gates**:
- Code Coverage: > 80%
- Duplicated Lines: < 3%
- Maintainability Rating: A
- Reliability Rating: A
- Security Rating: A
- Security Hotspots: All reviewed

**Runs On**:
- Every push to `main` and `develop` branches
- Every pull request to `main`

### 2. OWASP ZAP - Dynamic Application Security Testing

**Purpose**: Perform dynamic security testing on running applications to identify runtime vulnerabilities.

**Configuration**: `.github/workflows/security-scan.yml` and `.github/workflows/security-scan-zap.yml`

**Scan Types**:

#### Baseline Scan
- Quick passive scan
- Identifies common vulnerabilities
- Runs on every deployment
- Low false positive rate

#### Full Scan
- Active scanning with spider
- Comprehensive vulnerability detection
- Runs weekly or on-demand
- May generate false positives

#### API Scan
- OpenAPI/Swagger specification-based
- Tests all API endpoints
- Validates authentication and authorization
- Checks for API-specific vulnerabilities

**ZAP Rules Configuration**: `.zap/rules.tsv`

**Tested Vulnerabilities**:
- SQL Injection
- Cross-Site Scripting (XSS)
- Cross-Site Request Forgery (CSRF)
- Authentication and session management issues
- Security header misconfigurations
- Path traversal
- Remote code execution
- Information disclosure

**Runs On**:
- Weekly scheduled scans (Sundays at 2 AM UTC)
- Manual workflow dispatch
- After deployments to staging/production

### 3. Snyk - Dependency Vulnerability Scanning

**Purpose**: Identify known vulnerabilities in NuGet packages and dependencies.

**Configuration**: `.github/workflows/security-scan.yml` and `.github/workflows/ci.yml`

**Features**:
- Real-time vulnerability database
- Automated fix suggestions
- License compliance checking
- Dependency tree analysis
- Integration with GitHub Security tab

**Setup Instructions**:

1. Create a free account at [Snyk](https://snyk.io)
2. Get your API token from Account Settings
3. Add `SNYK_TOKEN` to GitHub repository secrets
4. Snyk will automatically scan on every push

**Severity Thresholds**:
- Critical: Fail build
- High: Fail build
- Medium: Warning
- Low: Informational

**Runs On**:
- Every push to any branch
- Every pull request
- Daily scheduled scans

### 4. OWASP Dependency Check

**Purpose**: Identify known vulnerabilities in project dependencies using the National Vulnerability Database (NVD).

**Configuration**: `.github/workflows/security-scan.yml` and `.github/dependency-check-suppressions.xml`

**Features**:
- NVD CVE database integration
- Retired dependency detection
- Experimental vulnerability detection
- CVSS score-based failure thresholds
- Suppression file for false positives

**Failure Threshold**: CVSS score >= 7.0 (High severity)

**Suppressions**: Managed in `.github/dependency-check-suppressions.xml`

**Runs On**:
- Every security scan workflow execution
- Daily scheduled scans

### 5. GitHub CodeQL - Semantic Code Analysis

**Purpose**: Perform deep semantic analysis to detect security vulnerabilities and coding errors.

**Configuration**: `.github/workflows/security-scan.yml` and `.github/workflows/ci.yml`

**Query Suites**:
- `security-extended`: Extended security queries
- `security-and-quality`: Combined security and quality queries

**Detected Issues**:
- SQL injection vulnerabilities
- Command injection
- Path traversal
- XSS vulnerabilities
- Insecure deserialization
- Cryptographic weaknesses
- Authentication bypass
- Authorization issues

**Runs On**:
- Every push to `main` and `develop`
- Every pull request
- Results appear in Security > Code scanning alerts

### 6. Trivy - Container Security Scanning

**Purpose**: Scan Docker container images for vulnerabilities and misconfigurations.

**Configuration**: `.github/workflows/security-scan.yml`

**Scans**:
- OS package vulnerabilities
- Application dependency vulnerabilities
- Container misconfigurations
- Secrets in container images

**Severity Filter**: CRITICAL and HIGH only

**Runs On**:
- When Docker containers are built
- Before deployment to production

### 7. SSL/TLS Configuration Testing

**Purpose**: Validate SSL/TLS configuration and identify protocol vulnerabilities.

**Tool**: testssl.sh

**Configuration**: `.github/workflows/security-scan.yml`

**Checks**:
- TLS version support (TLS 1.2+)
- Cipher suite configuration
- Certificate validation
- Protocol vulnerabilities (POODLE, BEAST, etc.)
- Perfect Forward Secrecy
- HSTS configuration

**Runs On**:
- Daily scheduled scans
- After infrastructure changes
- Manual workflow dispatch

### 8. Security Headers Validation

**Purpose**: Verify that all required security headers are properly configured.

**Configuration**: `.github/workflows/security-scan.yml`

**Required Headers**:
- `Content-Security-Policy`: XSS protection
- `X-Frame-Options`: Clickjacking protection
- `X-Content-Type-Options`: MIME sniffing protection
- `Strict-Transport-Security`: HTTPS enforcement
- `Referrer-Policy`: Referrer information control
- `Permissions-Policy`: Feature policy control

**Runs On**:
- Daily scheduled scans
- After application deployments

## CI/CD Integration

### Build Pipeline Integration

The main CI pipeline (`.github/workflows/ci.yml`) includes:

1. **Build and Test**: Compile code and run unit tests
2. **Security Scan**: Run SonarCloud, Snyk, CodeQL, and OWASP Dependency Check
3. **Infrastructure Validation**: Validate Terraform configurations
4. **Code Quality**: Additional quality checks

### Security Scan Pipeline

The dedicated security scan pipeline (`.github/workflows/security-scan.yml`) includes:

1. **Dependency Scan**: Snyk + OWASP Dependency Check
2. **Static Code Analysis**: CodeQL with security queries
3. **OWASP ZAP Scan**: Dynamic application testing
4. **Container Security**: Trivy container scanning
5. **SSL/TLS Scan**: testssl.sh configuration testing
6. **Security Headers**: Header validation
7. **Report Generation**: Consolidated security report

### Workflow Triggers

- **On Push**: `main`, `develop` branches
- **On Pull Request**: All PRs to `main` and `develop`
- **Scheduled**: Daily at 2 AM UTC
- **Manual**: Workflow dispatch with environment selection

## Security Reports and Artifacts

### Report Locations

1. **SonarCloud Dashboard**: https://sonarcloud.io/project/overview?id=PortfolioCMS
2. **GitHub Security Tab**: Security > Code scanning alerts
3. **Workflow Artifacts**: Download from Actions > Workflow run
4. **Azure Security Center**: Integrated security findings

### Artifact Types

- **OWASP ZAP Reports**: HTML, JSON, and Markdown formats
- **Dependency Check Reports**: HTML and JSON formats
- **SSL/TLS Results**: JSON format
- **Security Headers**: Text format
- **SARIF Files**: Uploaded to GitHub Security tab

### Report Retention

- Workflow artifacts: 90 days
- GitHub Security alerts: Indefinite
- SonarCloud history: Indefinite

## Compliance and Standards

### Security Standards

- **OWASP Top 10**: All OWASP Top 10 vulnerabilities are tested
- **CWE Top 25**: Common Weakness Enumeration coverage
- **SANS Top 25**: Most dangerous software errors
- **PCI DSS**: Payment Card Industry Data Security Standard considerations
- **GDPR**: Data protection and privacy requirements

### Quality Gates

All security scans must pass before deployment:

1. **SonarCloud Quality Gate**: Must pass
2. **Snyk Vulnerability Check**: No high/critical vulnerabilities
3. **OWASP Dependency Check**: CVSS < 7.0
4. **CodeQL**: No high-severity issues
5. **ZAP Scan**: No high-risk vulnerabilities

### Failure Handling

- **Critical Issues**: Build fails, deployment blocked
- **High Issues**: Build fails, requires review
- **Medium Issues**: Warning, requires tracking
- **Low Issues**: Informational, no action required

## Security Scan Schedule

| Scan Type | Frequency | Trigger |
|-----------|-----------|---------|
| SonarCloud | On push/PR | Automatic |
| Snyk | On push/PR + Daily | Automatic |
| OWASP Dependency Check | Daily | Scheduled |
| CodeQL | On push/PR | Automatic |
| OWASP ZAP Baseline | Weekly | Scheduled |
| OWASP ZAP Full | On-demand | Manual |
| Trivy | On container build | Automatic |
| SSL/TLS | Daily | Scheduled |
| Security Headers | Daily | Scheduled |

## Remediation Process

### 1. Vulnerability Detection

- Security scan identifies vulnerability
- Alert created in GitHub Security tab
- Notification sent to security team

### 2. Triage

- Review vulnerability details
- Assess severity and impact
- Determine if it's a true positive
- Assign priority and owner

### 3. Remediation

- Update vulnerable dependency
- Apply security patch
- Implement code fix
- Add suppression if false positive

### 4. Verification

- Re-run security scans
- Verify fix effectiveness
- Update documentation
- Close security alert

### 5. Prevention

- Update security policies
- Add new test cases
- Improve code review process
- Update developer training

## False Positive Management

### Suppression Files

- **OWASP Dependency Check**: `.github/dependency-check-suppressions.xml`
- **ZAP Rules**: `.zap/rules.tsv`
- **SonarCloud**: Configure in SonarCloud dashboard

### Suppression Guidelines

1. Document reason for suppression
2. Include expiration date
3. Get security team approval
4. Review suppressions quarterly
5. Remove when no longer needed

## Best Practices

### For Developers

1. **Run scans locally**: Use SonarCloud and Snyk CLI tools
2. **Fix issues early**: Address security issues in development
3. **Review scan results**: Check security scan results in PRs
4. **Update dependencies**: Keep dependencies up to date
5. **Follow secure coding**: Use OWASP secure coding practices

### For Security Team

1. **Monitor dashboards**: Regular review of security dashboards
2. **Triage alerts**: Prioritize and assign security alerts
3. **Update policies**: Keep security policies current
4. **Train developers**: Provide security training
5. **Audit scans**: Regular audit of scan configurations

### For DevOps

1. **Maintain pipelines**: Keep security scanning tools updated
2. **Monitor performance**: Ensure scans don't slow down CI/CD
3. **Manage secrets**: Securely manage API tokens
4. **Archive reports**: Maintain security scan history
5. **Automate remediation**: Implement automated fixes where possible

## Troubleshooting

### Common Issues

#### SonarCloud Scan Fails

- **Cause**: Missing or invalid SONAR_TOKEN
- **Solution**: Verify token in GitHub secrets, regenerate if needed

#### Snyk Scan Fails

- **Cause**: Missing SNYK_TOKEN or rate limit exceeded
- **Solution**: Add token to secrets, upgrade Snyk plan if needed

#### ZAP Scan Times Out

- **Cause**: Application not responding or scan too aggressive
- **Solution**: Adjust ZAP scan options, increase timeout

#### False Positives

- **Cause**: Scanner misidentifies code pattern
- **Solution**: Add suppression with documentation

#### Build Blocked by Security Gate

- **Cause**: High/critical vulnerability detected
- **Solution**: Fix vulnerability or get security team approval

## Additional Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [Snyk Documentation](https://docs.snyk.io/)
- [OWASP ZAP Documentation](https://www.zaproxy.org/docs/)
- [GitHub Security Features](https://docs.github.com/en/code-security)
- [OWASP Dependency Check](https://owasp.org/www-project-dependency-check/)

## Contact

For security-related questions or to report vulnerabilities:

- **Security Team**: security@example.com
- **GitHub Security Advisories**: Use GitHub's private vulnerability reporting
- **Emergency**: Follow incident response procedures in SECURITY.md

---

**Last Updated**: 2024-01-15
**Review Schedule**: Quarterly
**Document Owner**: Security Team
