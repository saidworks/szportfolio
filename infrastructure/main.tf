# Configure the Azure Provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
  }

  # Configure backend for state management in Azure Storage
  backend "azurerm" {
    resource_group_name  = "rg-portfoliocms-tfstate"
    storage_account_name = "stportfoliocmstfstate"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

# Create Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = var.common_tags
}

# Create App Service Plan
resource "azurerm_service_plan" "main" {
  name                = var.app_service_plan_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.app_service_plan_sku

  tags = var.common_tags
}

# Create App Service for Frontend
resource "azurerm_linux_web_app" "frontend" {
  name                = var.frontend_app_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id

  # Enable system-assigned managed identity for Key Vault access
  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on = var.environment == "production" ? true : false

    application_stack {
      dotnet_version = "9.0"
    }

    app_command_line = ""
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                = var.environment
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.main.connection_string
    "API_BASE_URL"                          = "https://${var.api_app_name}.azurewebsites.net"
    "KeyVault__VaultUri"                    = azurerm_key_vault.main.vault_uri
  }

  tags = var.common_tags
}

# Create App Service for API
resource "azurerm_linux_web_app" "api" {
  name                = var.api_app_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id

  # Enable system-assigned managed identity for Key Vault access
  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on = var.environment == "production" ? true : false

    application_stack {
      dotnet_version = "9.0"
    }

    cors {
      allowed_origins = [
        "https://${var.frontend_app_name}.azurewebsites.net"
      ]
      support_credentials = true
    }
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                = var.environment
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.main.connection_string
    "KeyVault__VaultUri"                    = azurerm_key_vault.main.vault_uri
    # Note: Connection string will be retrieved from Key Vault at runtime using managed identity
    # Keeping this for backward compatibility during migration
    "ConnectionStrings__DefaultConnection" = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }

  tags = var.common_tags
}

# Create SQL Server
resource "azurerm_mssql_server" "main" {
  name                         = var.sql_server_name
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password

  tags = var.common_tags
}

# Create SQL Database
resource "azurerm_mssql_database" "main" {
  name           = var.sql_database_name
  server_id      = azurerm_mssql_server.main.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = var.sql_database_max_size_gb
  sku_name       = var.sql_database_sku
  zone_redundant = var.environment == "production" ? true : false

  # Enable automatic backups
  # Note: Free tier supports 7 days retention, paid tiers support up to 35 days
  short_term_retention_policy {
    retention_days = var.sql_database_sku == "Free" ? 7 : 35
  }

  tags = var.common_tags
}

# Create SQL Server Firewall Rule for Azure Services
resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Create Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "main" {
  name                = var.log_analytics_workspace_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = var.log_analytics_retention_days

  tags = var.common_tags
}

# Create Application Insights
resource "azurerm_application_insights" "main" {
  name                = var.application_insights_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  retention_in_days   = var.application_insights_retention_days

  tags = var.common_tags
}

# Create Action Group for Alerts
resource "azurerm_monitor_action_group" "main" {
  name                = "${var.application_insights_name}-action-group"
  resource_group_name = azurerm_resource_group.main.name
  short_name          = "portfolioag"

  email_receiver {
    name                    = "admin-email"
    email_address           = var.alert_email_address
    use_common_alert_schema = true
  }

  tags = var.common_tags
}

# Create Metric Alert for High Response Time
resource "azurerm_monitor_metric_alert" "high_response_time" {
  name                = "${var.application_insights_name}-high-response-time"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_application_insights.main.id]
  description         = "Alert when average response time exceeds threshold"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "requests/duration"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = var.response_time_threshold_ms
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.common_tags
}

# Create Metric Alert for High Error Rate
resource "azurerm_monitor_metric_alert" "high_error_rate" {
  name                = "${var.application_insights_name}-high-error-rate"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_application_insights.main.id]
  description         = "Alert when error rate exceeds threshold"
  severity            = 1
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "requests/failed"
    aggregation      = "Count"
    operator         = "GreaterThan"
    threshold        = var.error_rate_threshold
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.common_tags
}

# Create Metric Alert for Database DTU Usage
resource "azurerm_monitor_metric_alert" "high_dtu_usage" {
  name                = "${var.sql_database_name}-high-dtu-usage"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_mssql_database.main.id]
  description         = "Alert when database DTU usage exceeds threshold"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.Sql/servers/databases"
    metric_name      = "dtu_consumption_percent"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = var.dtu_threshold_percent
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.common_tags
}

# Create Metric Alert for Storage Account Availability
resource "azurerm_monitor_metric_alert" "storage_availability" {
  name                = "${var.storage_account_name}-low-availability"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_storage_account.media.id]
  description         = "Alert when storage account availability drops"
  severity            = 1
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.Storage/storageAccounts"
    metric_name      = "Availability"
    aggregation      = "Average"
    operator         = "LessThan"
    threshold        = 99.0
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.common_tags
}

# Create Application Insights Web Test for Frontend
resource "azurerm_application_insights_standard_web_test" "frontend" {
  name                    = "${var.application_insights_name}-frontend-webtest"
  resource_group_name     = azurerm_resource_group.main.name
  location                = azurerm_resource_group.main.location
  application_insights_id = azurerm_application_insights.main.id
  geo_locations           = var.webtest_geo_locations
  frequency               = 300
  timeout                 = 30
  enabled                 = true

  request {
    url = "https://${azurerm_linux_web_app.frontend.default_hostname}"
  }

  validation_rules {
    expected_status_code = 200
  }

  tags = var.common_tags
}

# Create Application Insights Web Test for API
resource "azurerm_application_insights_standard_web_test" "api" {
  name                    = "${var.application_insights_name}-api-webtest"
  resource_group_name     = azurerm_resource_group.main.name
  location                = azurerm_resource_group.main.location
  application_insights_id = azurerm_application_insights.main.id
  geo_locations           = var.webtest_geo_locations
  frequency               = 300
  timeout                 = 30
  enabled                 = true

  request {
    url = "https://${azurerm_linux_web_app.api.default_hostname}/api/v1/health"
  }

  validation_rules {
    expected_status_code = 200
  }

  tags = var.common_tags
}

# Create Metric Alert for Web Test Failures
resource "azurerm_monitor_metric_alert" "webtest_failures" {
  name                = "${var.application_insights_name}-webtest-failures"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_application_insights.main.id]
  description         = "Alert when web tests fail"
  severity            = 1
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "availabilityResults/availabilityPercentage"
    aggregation      = "Average"
    operator         = "LessThan"
    threshold        = 90.0
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.common_tags
}

# Create Storage Account for media files
resource "azurerm_storage_account" "media" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = var.environment == "production" ? "GRS" : "LRS"

  blob_properties {
    cors_rule {
      allowed_headers    = ["*"]
      allowed_methods    = ["GET", "HEAD", "POST", "PUT"]
      allowed_origins    = ["https://${var.frontend_app_name}.azurewebsites.net"]
      exposed_headers    = ["*"]
      max_age_in_seconds = 3600
    }
  }

  tags = var.common_tags
}

# Create Storage Container for media files
resource "azurerm_storage_container" "media" {
  name                  = "media"
  storage_account_name  = azurerm_storage_account.media.name
  container_access_type = "blob"
}

# Create Key Vault
resource "azurerm_key_vault" "main" {
  name                        = var.key_vault_name
  location                    = azurerm_resource_group.main.location
  resource_group_name         = azurerm_resource_group.main.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  sku_name                    = "standard"

  # Access policy for the current user/service principal running Terraform
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Get",
    ]

    secret_permissions = [
      "Get", "List", "Set", "Delete", "Purge", "Recover"
    ]

    storage_permissions = [
      "Get",
    ]
  }

  tags = var.common_tags
}

# Grant Frontend App Service managed identity access to Key Vault
resource "azurerm_key_vault_access_policy" "frontend" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.frontend.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]

  depends_on = [azurerm_key_vault.main, azurerm_linux_web_app.frontend]
}

# Grant API App Service managed identity access to Key Vault
resource "azurerm_key_vault_access_policy" "api" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.api.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]

  depends_on = [azurerm_key_vault.main, azurerm_linux_web_app.api]
}

# Get current client configuration
data "azurerm_client_config" "current" {}

# Store SQL admin password in Key Vault
# Note: For production, this should be created manually via Azure CLI before Terraform deployment
# to avoid storing sensitive passwords in Terraform state
resource "azurerm_key_vault_secret" "sql_admin_password" {
  name         = "SqlAdminPassword"
  value        = var.sql_admin_password
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault.main]

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually rotated passwords
  }
}

# Store SQL connection string in Key Vault
resource "azurerm_key_vault_secret" "sql_connection_string" {
  name         = "SqlConnectionString"
  value        = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault.main]

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually rotated connection strings
  }
}

# Store Storage Account connection string in Key Vault
resource "azurerm_key_vault_secret" "storage_connection_string" {
  name         = "StorageConnectionString"
  value        = azurerm_storage_account.media.primary_connection_string
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault.main]
}

# Create CDN Profile for static asset delivery
resource "azurerm_cdn_profile" "main" {
  name                = var.cdn_profile_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = var.cdn_sku

  tags = var.common_tags
}

# Create CDN Endpoint for Frontend App
resource "azurerm_cdn_endpoint" "frontend" {
  name                = "${var.cdn_endpoint_name}-frontend"
  profile_name        = azurerm_cdn_profile.main.name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  is_http_allowed     = false
  is_https_allowed    = true

  origin {
    name      = "frontend-origin"
    host_name = azurerm_linux_web_app.frontend.default_hostname
  }

  delivery_rule {
    name  = "EnforceHTTPS"
    order = 1

    request_scheme_condition {
      operator     = "Equal"
      match_values = ["HTTP"]
    }

    url_redirect_action {
      redirect_type = "Found"
      protocol      = "Https"
    }
  }

  tags = var.common_tags
}

# Create CDN Endpoint for Media Storage
resource "azurerm_cdn_endpoint" "media" {
  name                = "${var.cdn_endpoint_name}-media"
  profile_name        = azurerm_cdn_profile.main.name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  is_http_allowed     = false
  is_https_allowed    = true

  origin {
    name      = "media-origin"
    host_name = azurerm_storage_account.media.primary_blob_host
  }

  delivery_rule {
    name  = "CacheStaticAssets"
    order = 1

    url_file_extension_condition {
      operator     = "Equal"
      match_values = ["jpg", "jpeg", "png", "gif", "svg", "css", "js", "woff", "woff2", "ttf", "eot"]
    }

    cache_expiration_action {
      behavior = "Override"
      duration = "7.00:00:00" # 7 days
    }
  }

  tags = var.common_tags
}

# Create Virtual Network for Application Gateway
resource "azurerm_virtual_network" "main" {
  count               = var.enable_application_gateway ? 1 : 0
  name                = var.vnet_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = var.vnet_address_space

  tags = var.common_tags
}

# Create Subnet for Application Gateway
resource "azurerm_subnet" "appgw" {
  count                = var.enable_application_gateway ? 1 : 0
  name                 = "${var.subnet_name}-appgw"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main[0].name
  address_prefixes     = var.appgw_subnet_address_prefixes
}

# Create Public IP for Application Gateway
resource "azurerm_public_ip" "appgw" {
  count               = var.enable_application_gateway ? 1 : 0
  name                = "${var.application_gateway_name}-pip"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  allocation_method   = "Static"
  sku                 = "Standard"

  tags = var.common_tags
}

# Create Application Gateway with WAF
resource "azurerm_application_gateway" "main" {
  count               = var.enable_application_gateway ? 1 : 0
  name                = var.application_gateway_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  sku {
    name     = var.application_gateway_sku
    tier     = var.application_gateway_tier
    capacity = var.application_gateway_capacity
  }

  gateway_ip_configuration {
    name      = "appgw-ip-config"
    subnet_id = azurerm_subnet.appgw[0].id
  }

  frontend_port {
    name = "https-port"
    port = 443
  }

  frontend_port {
    name = "http-port"
    port = 80
  }

  frontend_ip_configuration {
    name                 = "appgw-frontend-ip"
    public_ip_address_id = azurerm_public_ip.appgw[0].id
  }

  backend_address_pool {
    name  = "frontend-backend-pool"
    fqdns = [azurerm_linux_web_app.frontend.default_hostname]
  }

  backend_address_pool {
    name  = "api-backend-pool"
    fqdns = [azurerm_linux_web_app.api.default_hostname]
  }

  backend_http_settings {
    name                                = "https-settings"
    cookie_based_affinity               = "Disabled"
    port                                = 443
    protocol                            = "Https"
    request_timeout                     = 60
    pick_host_name_from_backend_address = true
  }

  http_listener {
    name                           = "http-listener"
    frontend_ip_configuration_name = "appgw-frontend-ip"
    frontend_port_name             = "http-port"
    protocol                       = "Http"
  }

  http_listener {
    name                           = "https-listener"
    frontend_ip_configuration_name = "appgw-frontend-ip"
    frontend_port_name             = "https-port"
    protocol                       = "Https"
    ssl_certificate_name           = "appgw-ssl-cert"
  }

  ssl_certificate {
    name     = "appgw-ssl-cert"
    data     = var.ssl_certificate_data
    password = var.ssl_certificate_password
  }

  request_routing_rule {
    name                        = "http-to-https-redirect"
    rule_type                   = "Basic"
    http_listener_name          = "http-listener"
    redirect_configuration_name = "http-to-https"
    priority                    = 100
  }

  request_routing_rule {
    name               = "frontend-routing-rule"
    rule_type          = "PathBasedRouting"
    http_listener_name = "https-listener"
    url_path_map_name  = "path-map"
    priority           = 200
  }

  redirect_configuration {
    name                 = "http-to-https"
    redirect_type        = "Permanent"
    target_listener_name = "https-listener"
    include_path         = true
    include_query_string = true
  }

  url_path_map {
    name                               = "path-map"
    default_backend_address_pool_name  = "frontend-backend-pool"
    default_backend_http_settings_name = "https-settings"

    path_rule {
      name                       = "api-path-rule"
      paths                      = ["/api/*"]
      backend_address_pool_name  = "api-backend-pool"
      backend_http_settings_name = "https-settings"
    }
  }

  # WAF Configuration
  waf_configuration {
    enabled          = true
    firewall_mode    = var.waf_mode
    rule_set_type    = "OWASP"
    rule_set_version = "3.2"

    disabled_rule_group {
      rule_group_name = "REQUEST-942-APPLICATION-ATTACK-SQLI"
      rules           = []
    }
  }

  tags = var.common_tags
}