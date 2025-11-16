# Azure Security Services Setup Script
# This script automates the setup of Azure Key Vault, Managed Identity, and security configurations

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlDatabaseName,
    
    [Parameter(Mandatory=$true)]
    [string]$FrontendAppName,
    
    [Parameter(Mandatory=$true)]
    [string]$ApiAppName,
    
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "development",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipSecretGeneration
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Azure Security Services Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to generate secure password
function New-SecurePassword {
    param([int]$Length = 32)
    
    $chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
    $password = -join ((1..$Length) | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })
    return $password
}

# Function to check if Azure CLI is installed
function Test-AzureCLI {
    try {
        $null = az --version
        return $true
    }
    catch {
        return $false
    }
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

if (-not (Test-AzureCLI)) {
    Write-Host "ERROR: Azure CLI is not installed. Please install it from https://aka.ms/installazurecliwindows" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Azure CLI is installed" -ForegroundColor Green

# Login check
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json

if (-not $account) {
    Write-Host "Not logged in to Azure. Please login..." -ForegroundColor Yellow
    az login
    $account = az account show | ConvertFrom-Json
}

Write-Host "✓ Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host "✓ Subscription: $($account.name)" -ForegroundColor Green
Write-Host ""

# Step 1: Verify Resource Group exists
Write-Host "Step 1: Verifying Resource Group..." -ForegroundColor Cyan
$rg = az group show --name $ResourceGroupName 2>$null | ConvertFrom-Json

if (-not $rg) {
    Write-Host "ERROR: Resource Group '$ResourceGroupName' not found. Please create it first or run Terraform." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Resource Group exists: $ResourceGroupName" -ForegroundColor Green
Write-Host ""

# Step 2: Verify Key Vault exists
Write-Host "Step 2: Verifying Key Vault..." -ForegroundColor Cyan
$kv = az keyvault show --name $KeyVaultName --resource-group $ResourceGroupName 2>$null | ConvertFrom-Json

if (-not $kv) {
    Write-Host "ERROR: Key Vault '$KeyVaultName' not found. Please run Terraform first." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Key Vault exists: $KeyVaultName" -ForegroundColor Green
Write-Host "✓ Key Vault URI: $($kv.properties.vaultUri)" -ForegroundColor Green
Write-Host ""

# Step 3: Generate and store secrets
if (-not $SkipSecretGeneration) {
    Write-Host "Step 3: Generating and storing secrets..." -ForegroundColor Cyan
    
    # Generate SQL admin password
    Write-Host "  Generating SQL admin password..." -ForegroundColor Yellow
    $sqlPassword = New-SecurePassword -Length 32
    
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name "SqlAdminPassword" `
        --value $sqlPassword `
        --output none
    
    Write-Host "  ✓ SQL admin password stored in Key Vault" -ForegroundColor Green
    
    # Generate JWT secret key
    Write-Host "  Generating JWT secret key..." -ForegroundColor Yellow
    $jwtSecret = New-SecurePassword -Length 64
    
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name "JwtSecretKey" `
        --value $jwtSecret `
        --output none
    
    Write-Host "  ✓ JWT secret key stored in Key Vault" -ForegroundColor Green
    
    Write-Host ""
}
else {
    Write-Host "Step 3: Skipping secret generation (--SkipSecretGeneration flag set)" -ForegroundColor Yellow
    Write-Host ""
}

# Step 4: Store connection strings
Write-Host "Step 4: Storing connection strings..." -ForegroundColor Cyan

# Get SQL Server FQDN
Write-Host "  Retrieving SQL Server information..." -ForegroundColor Yellow
$sqlServer = az sql server show `
    --resource-group $ResourceGroupName `
    --name $SqlServerName `
    2>$null | ConvertFrom-Json

if (-not $sqlServer) {
    Write-Host "  WARNING: SQL Server '$SqlServerName' not found. Skipping SQL connection string." -ForegroundColor Yellow
}
else {
    $sqlServerFqdn = $sqlServer.fullyQualifiedDomainName
    
    # Get SQL password from Key Vault
    $sqlPassword = az keyvault secret show `
        --vault-name $KeyVaultName `
        --name "SqlAdminPassword" `
        --query "value" `
        --output tsv
    
    # Create connection string
    $sqlConnectionString = "Server=tcp:$sqlServerFqdn,1433;Initial Catalog=$SqlDatabaseName;Persist Security Info=False;User ID=sqladmin;Password=$sqlPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name "SqlConnectionString" `
        --value $sqlConnectionString `
        --output none
    
    Write-Host "  ✓ SQL connection string stored in Key Vault" -ForegroundColor Green
}

# Get Storage Account connection string
Write-Host "  Retrieving Storage Account information..." -ForegroundColor Yellow
$storageAccount = az storage account show `
    --resource-group $ResourceGroupName `
    --name $StorageAccountName `
    2>$null | ConvertFrom-Json

if (-not $storageAccount) {
    Write-Host "  WARNING: Storage Account '$StorageAccountName' not found. Skipping storage connection string." -ForegroundColor Yellow
}
else {
    $storageConnectionString = az storage account show-connection-string `
        --resource-group $ResourceGroupName `
        --name $StorageAccountName `
        --query "connectionString" `
        --output tsv
    
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name "StorageConnectionString" `
        --value $storageConnectionString `
        --output none
    
    Write-Host "  ✓ Storage connection string stored in Key Vault" -ForegroundColor Green
}

Write-Host ""

# Step 5: Verify Managed Identities
Write-Host "Step 5: Verifying Managed Identities..." -ForegroundColor Cyan

# Check Frontend App
Write-Host "  Checking Frontend App managed identity..." -ForegroundColor Yellow
$frontendApp = az webapp show `
    --resource-group $ResourceGroupName `
    --name $FrontendAppName `
    2>$null | ConvertFrom-Json

if (-not $frontendApp) {
    Write-Host "  WARNING: Frontend App '$FrontendAppName' not found." -ForegroundColor Yellow
}
elseif (-not $frontendApp.identity) {
    Write-Host "  WARNING: Frontend App does not have managed identity enabled." -ForegroundColor Yellow
}
else {
    $frontendIdentity = $frontendApp.identity.principalId
    Write-Host "  ✓ Frontend App managed identity: $frontendIdentity" -ForegroundColor Green
}

# Check API App
Write-Host "  Checking API App managed identity..." -ForegroundColor Yellow
$apiApp = az webapp show `
    --resource-group $ResourceGroupName `
    --name $ApiAppName `
    2>$null | ConvertFrom-Json

if (-not $apiApp) {
    Write-Host "  WARNING: API App '$ApiAppName' not found." -ForegroundColor Yellow
}
elseif (-not $apiApp.identity) {
    Write-Host "  WARNING: API App does not have managed identity enabled." -ForegroundColor Yellow
}
else {
    $apiIdentity = $apiApp.identity.principalId
    Write-Host "  ✓ API App managed identity: $apiIdentity" -ForegroundColor Green
}

Write-Host ""

# Step 6: Verify Key Vault Access Policies
Write-Host "Step 6: Verifying Key Vault access policies..." -ForegroundColor Cyan

$accessPolicies = az keyvault show `
    --name $KeyVaultName `
    --resource-group $ResourceGroupName `
    --query "properties.accessPolicies" | ConvertFrom-Json

$frontendHasAccess = $accessPolicies | Where-Object { $_.objectId -eq $frontendIdentity }
$apiHasAccess = $accessPolicies | Where-Object { $_.objectId -eq $apiIdentity }

if ($frontendHasAccess) {
    Write-Host "  ✓ Frontend App has Key Vault access" -ForegroundColor Green
}
else {
    Write-Host "  WARNING: Frontend App does not have Key Vault access policy" -ForegroundColor Yellow
}

if ($apiHasAccess) {
    Write-Host "  ✓ API App has Key Vault access" -ForegroundColor Green
}
else {
    Write-Host "  WARNING: API App does not have Key Vault access policy" -ForegroundColor Yellow
}

Write-Host ""

# Step 7: List all secrets
Write-Host "Step 7: Verifying stored secrets..." -ForegroundColor Cyan

$secrets = az keyvault secret list --vault-name $KeyVaultName --query "[].name" --output tsv

$requiredSecrets = @(
    "SqlAdminPassword",
    "SqlConnectionString",
    "JwtSecretKey",
    "StorageConnectionString"
)

foreach ($secretName in $requiredSecrets) {
    if ($secrets -contains $secretName) {
        Write-Host "  ✓ Secret exists: $secretName" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ Secret missing: $secretName" -ForegroundColor Red
    }
}

Write-Host ""

# Step 8: Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resource Group:    $ResourceGroupName" -ForegroundColor White
Write-Host "Key Vault:         $KeyVaultName" -ForegroundColor White
Write-Host "Key Vault URI:     $($kv.properties.vaultUri)" -ForegroundColor White
Write-Host "SQL Server:        $SqlServerName" -ForegroundColor White
Write-Host "Frontend App:      $FrontendAppName" -ForegroundColor White
Write-Host "API App:           $ApiAppName" -ForegroundColor White
Write-Host "Storage Account:   $StorageAccountName" -ForegroundColor White
Write-Host "Environment:       $Environment" -ForegroundColor White
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Update appsettings.json with Key Vault URI: $($kv.properties.vaultUri)" -ForegroundColor White
Write-Host "2. Deploy applications to Azure App Services" -ForegroundColor White
Write-Host "3. Verify applications can access Key Vault secrets" -ForegroundColor White
Write-Host "4. Configure Application Gateway WAF (if enabled)" -ForegroundColor White
Write-Host "5. Set up monitoring and alerting" -ForegroundColor White
Write-Host ""

Write-Host "For detailed instructions, see:" -ForegroundColor Yellow
Write-Host "  docs/security/AZURE_SECURITY_SETUP.md" -ForegroundColor White
Write-Host "  docs/security/SECURITY_CONFIGURATION.md" -ForegroundColor White
Write-Host ""

Write-Host "Setup completed successfully!" -ForegroundColor Green
