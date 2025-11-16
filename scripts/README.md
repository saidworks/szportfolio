# Scripts Directory

This directory contains **template scripts** with placeholder values for version control. These scripts are safe to commit to the repository.

## ⚠️ Important: Working with Scripts

### Template Scripts (This Directory)
- **Purpose**: Version-controlled templates with placeholder values
- **Location**: `scripts/`
- **Naming**: `*.template.ps1` or descriptive names
- **Content**: Placeholders like `<YOUR_VALUE>`, `<SUBSCRIPTION_ID>`, etc.
- **Safe to commit**: ✅ YES

### Working Scripts (Not in Version Control)
- **Purpose**: Actual scripts with real credentials and values
- **Location**: `.infra/scripts/`
- **Naming**: Same as templates but without `.template` suffix
- **Content**: Real values, passwords, connection strings
- **Safe to commit**: ❌ NO - Directory is gitignored

## Workflow

### 1. Using Template Scripts

When you need to run a script:

1. **Copy template to `.infra/scripts/`**:
   ```powershell
   Copy-Item scripts/setup-azure-security.template.ps1 .infra/scripts/setup-azure-security.ps1
   ```

2. **Edit the copy** in `.infra/scripts/` and replace placeholders with actual values

3. **Run the script** from `.infra/scripts/`:
   ```powershell
   .\.infra\scripts\setup-azure-security.ps1 -ResourceGroupName "rg-portfoliocms-dev" ...
   ```

4. **Never commit** the file in `.infra/scripts/` to version control

### 2. Creating New Scripts

When creating a new script:

1. **Create working version** in `.infra/scripts/` with real values first
2. **Test thoroughly** to ensure it works
3. **Create template version** in `scripts/` by:
   - Copying the working script
   - Replacing all sensitive values with placeholders
   - Adding comments explaining what values are needed
   - Adding `.template` suffix to filename (optional but recommended)
4. **Commit only the template** to version control

## Available Scripts

### setup-azure-security.template.ps1
**Purpose**: Set up Azure Key Vault, managed identities, and security configurations

**Template Location**: `scripts/setup-azure-security.template.ps1`  
**Working Location**: `.infra/scripts/setup-azure-security.ps1` (create from template)

**Required Values**:
- `ResourceGroupName` - Azure resource group name
- `KeyVaultName` - Azure Key Vault name (globally unique)
- `SqlServerName` - SQL Server name (globally unique)
- `SqlDatabaseName` - SQL Database name
- `FrontendAppName` - Frontend App Service name (globally unique)
- `ApiAppName` - API App Service name (globally unique)
- `StorageAccountName` - Storage account name (globally unique)

**Usage**:
```powershell
# Copy template to .infra/
Copy-Item scripts/setup-azure-security.template.ps1 .infra/scripts/setup-azure-security.ps1

# Edit .infra/scripts/setup-azure-security.ps1 and replace placeholders

# Run the script
.\.infra\scripts\setup-azure-security.ps1 `
    -ResourceGroupName "rg-portfoliocms-dev" `
    -KeyVaultName "kv-portfoliocms-dev" `
    -SqlServerName "sql-portfoliocms-dev" `
    -SqlDatabaseName "sqldb-portfoliocms" `
    -FrontendAppName "app-portfoliocms-frontend-dev" `
    -ApiAppName "app-portfoliocms-api-dev" `
    -StorageAccountName "stportfoliocmsdev"
```

### security-pentest.ps1
**Purpose**: Run security penetration tests against deployed application

**Location**: `scripts/security-pentest.ps1` (ready to use, no secrets)

**Usage**:
```powershell
.\scripts\security-pentest.ps1 -BaseUrl "https://your-app.azurewebsites.net"
```

**Note**: This script doesn't contain secrets and can be run directly.

## Example Scripts in .infra/

The `.infra/scripts/` directory contains example scripts to help you get started:

### EXAMPLE-deploy-environment.ps1
A comprehensive example showing how to structure deployment scripts with:
- Azure login and subscription selection
- Resource group creation
- Key Vault setup and secret storage
- Terraform deployment
- Managed identity configuration
- Connection string management
- Deployment logging

**To use**:
1. Copy to a new file (e.g., `deploy-dev.ps1`)
2. Replace all placeholder values
3. Customize for your specific needs
4. Run and test

## Security Best Practices

### ✅ DO:
- Keep templates in `scripts/` directory
- Use descriptive placeholder names like `<YOUR_SUBSCRIPTION_ID>`
- Add comments explaining what each value should be
- Test scripts thoroughly before creating templates
- Document required permissions and prerequisites
- Use `.template` suffix for clarity

### ❌ DON'T:
- Commit files from `.infra/scripts/` to version control
- Hardcode secrets in template scripts
- Share `.infra/` scripts via email or chat
- Use production credentials in development scripts
- Leave credentials in terminal history or clipboard

## Verification

### Check if .infra/ is gitignored:
```powershell
git check-ignore .infra/
# Should output: .infra/
```

### Check for accidentally committed secrets:
```powershell
git log --all --full-history -- "*.tfvars"
git log --all --full-history -- "*password*"
git log --all --full-history -- "*secret*"
```

## Related Documentation

- [.infra/README.md](../.infra/README.md) - Comprehensive guide for .infra/ directory
- [infrastructure/README.md](../infrastructure/README.md) - Infrastructure deployment guide
- [docs/security/AZURE_SECURITY_SETUP.md](../docs/security/AZURE_SECURITY_SETUP.md) - Azure security configuration

## Support

For questions about scripts:
1. Review this README and `.infra/README.md`
2. Check example scripts in `.infra/scripts/`
3. Consult infrastructure documentation
4. Contact DevOps team

---

**Remember**: Templates in `scripts/` are safe to commit. Working scripts in `.infra/scripts/` should never be committed.
