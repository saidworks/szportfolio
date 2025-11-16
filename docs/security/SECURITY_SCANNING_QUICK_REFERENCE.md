# Security Scanning Quick Reference

Quick reference guide for security scanning tools in Portfolio CMS.

## 🚀 Quick Start (5 Minutes)

### Minimum Setup to Get Started

1. **SonarCloud** (2 min)
   ```bash
   # 1. Go to https://sonarcloud.io
   # 2. Sign in with GitHub
   # 3. Import repository
   # 4. Generate token
   # 5. Add SONAR_TOKEN to GitHub secrets
   ```

2. **Snyk** (2 min)
   ```bash
   # 1. Go to https://snyk.io
   # 2. Sign in with GitHub
   # 3. Import repository
   # 4. Copy API token
   # 5. Add SNYK_TOKEN to GitHub secrets
   ```

3. **Push Code** (1 min)
   ```bash
   git push
   # All scans will run automatically!
   ```

## 📊 Security Scanning Tools Overview

| Tool | Purpose | Setup Time | Auto-Run |
|------|---------|------------|----------|
| **SonarCloud** | Static code analysis | 2 min | ✅ On push/PR |
| **Snyk** | Dependency vulnerabilities | 2 min | ✅ On push/PR |
| **CodeQL** | Semantic security analysis | 0 min | ✅ Built-in |
| **OWASP Dependency Check** | CVE scanning | 0 min | ✅ Daily |
| **OWASP ZAP** | Dynamic app testing | 0 min | ✅ Weekly |
| **Trivy** | Container scanning | 0 min | ✅ On build |
| **testssl.sh** | SSL/TLS testing | 0 min | ✅ Daily |
| **Security Headers** | Header validation | 0 min | ✅ Daily |

## 🔍 Where to Find Results

### GitHub Security Tab
```
Repository → Security → Code scanning alerts
```
- CodeQL findings
- Snyk vulnerabilities
- Trivy container issues

### SonarCloud Dashboard
```
https://sonarcloud.io/project/overview?id=PortfolioCMS
```
- Code quality metrics
- Security hotspots
- Technical debt
- Code coverage

### Snyk Dashboard
```
https://app.snyk.io/
```
- Dependency vulnerabilities
- Fix recommendations
- License issues

### GitHub Actions Artifacts
```
Repository → Actions → Workflow run → Artifacts
```
- OWASP ZAP reports (HTML/JSON)
- Dependency Check reports
- SSL/TLS test results
- Security headers results

## 🎯 Common Commands

### Run Security Scan Manually
```bash
# Go to GitHub Actions
# Select "Security Scanning" workflow
# Click "Run workflow"
# Choose environment and scan type
```

### View Latest Scan Results
```bash
# GitHub CLI
gh run list --workflow=security-scan.yml --limit 1
gh run view <run-id>

# Or visit:
# https://github.com/your-org/portfolio-cms/actions
```

### Download Scan Reports
```bash
# GitHub CLI
gh run download <run-id>

# Or download from Actions → Artifacts
```

## 🚨 Understanding Severity Levels

| Severity | Action | Example |
|----------|--------|---------|
| **Critical** | ❌ Build fails | SQL injection vulnerability |
| **High** | ❌ Build fails | XSS vulnerability |
| **Medium** | ⚠️ Warning | Missing security header |
| **Low** | ℹ️ Info | Code smell |

## 🔧 Quick Fixes

### Fix Dependency Vulnerability
```bash
# Update package to latest version
dotnet add package PackageName --version x.x.x

# Or update all packages
dotnet outdated --upgrade
```

### Fix SonarCloud Issue
```bash
# 1. View issue in SonarCloud dashboard
# 2. Click on issue for details
# 3. Follow remediation guidance
# 4. Commit fix
# 5. Verify in next scan
```

### Suppress False Positive

**OWASP Dependency Check:**
```xml
<!-- .github/dependency-check-suppressions.xml -->
<suppress>
    <notes>Reason for suppression</notes>
    <packageUrl regex="true">^pkg:nuget/Package.*$</packageUrl>
    <cve>CVE-2024-XXXXX</cve>
</suppress>
```

**ZAP Rules:**
```tsv
# .zap/rules.tsv
10021	IGNORE	WARN	# Rule description
```

## 📅 Scan Schedule

| Day | Time (UTC) | Scans Running |
|-----|------------|---------------|
| **Daily** | 2:00 AM | Snyk, OWASP Dep Check, SSL/TLS, Headers |
| **Sunday** | 2:00 AM | OWASP ZAP Baseline + API |
| **On Push** | Immediate | SonarCloud, Snyk, CodeQL |
| **On PR** | Immediate | SonarCloud, Snyk, CodeQL |

## 🎓 Learning Resources

### Quick Reads (5-10 min each)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [SonarCloud Docs](https://docs.sonarcloud.io/)
- [Snyk Docs](https://docs.snyk.io/)

### Video Tutorials
- [SonarCloud Setup](https://www.youtube.com/results?search_query=sonarcloud+setup)
- [Snyk Tutorial](https://www.youtube.com/results?search_query=snyk+tutorial)
- [OWASP ZAP Basics](https://www.youtube.com/results?search_query=owasp+zap+tutorial)

## 🆘 Troubleshooting

### "SonarCloud scan failed"
```bash
# Check SONAR_TOKEN is set
# Verify organization key is correct
# Check SonarCloud service status
```

### "Snyk scan failed"
```bash
# Check SNYK_TOKEN is set
# Verify token hasn't expired
# Check Snyk rate limits
```

### "Too many false positives"
```bash
# Add suppressions to appropriate files
# Document reason for each suppression
# Review suppressions quarterly
```

### "Build blocked by security gate"
```bash
# Review security findings
# Fix high/critical issues
# Or request security team approval
```

## 📞 Getting Help

### Documentation
- Full docs: `docs/security/SECURITY_SCANNING.md`
- Setup guide: `docs/security/SECURITY_SCANNING_SETUP.md`
- Security policy: `SECURITY.md`

### Support Channels
- GitHub Issues: Report bugs or request features
- Security Team: security@example.com
- Emergency: Follow incident response in SECURITY.md

## ✅ Daily Checklist for Developers

- [ ] Check GitHub Security tab for new alerts
- [ ] Review SonarCloud quality gate on PRs
- [ ] Address high/critical Snyk vulnerabilities
- [ ] Keep dependencies up to date
- [ ] Run local scans before pushing

## 🎯 Weekly Checklist for Security Team

- [ ] Review OWASP ZAP scan results
- [ ] Triage new security alerts
- [ ] Update suppressions if needed
- [ ] Check scan performance metrics
- [ ] Update security documentation

## 📈 Metrics to Track

- **Security Rating**: Target A on SonarCloud
- **Vulnerabilities**: Target 0 high/critical
- **Code Coverage**: Target > 80%
- **Technical Debt**: Keep under 5 days
- **Security Hotspots**: All reviewed

---

**Last Updated**: 2024-01-15  
**Quick Reference Version**: 1.0  
**For detailed information, see**: `docs/security/SECURITY_SCANNING.md`
