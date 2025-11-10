# Resource Group Outputs
output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "resource_group_location" {
  description = "The location of the resource group"
  value       = azurerm_resource_group.main.location
}

# App Service Outputs
output "app_service_plan_id" {
  description = "The ID of the App Service Plan"
  value       = azurerm_service_plan.main.id
}

output "frontend_app_url" {
  description = "The URL of the Frontend App Service"
  value       = "https://${azurerm_linux_web_app.frontend.default_hostname}"
}

output "api_app_url" {
  description = "The URL of the API App Service"
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "frontend_app_name" {
  description = "The name of the Frontend App Service"
  value       = azurerm_linux_web_app.frontend.name
}

output "api_app_name" {
  description = "The name of the API App Service"
  value       = azurerm_linux_web_app.api.name
}

# SQL Database Outputs
output "sql_server_name" {
  description = "The name of the SQL Server"
  value       = azurerm_mssql_server.main.name
}

output "sql_server_fqdn" {
  description = "The fully qualified domain name of the SQL Server"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
  sensitive   = true
}

output "sql_database_name" {
  description = "The name of the SQL Database"
  value       = azurerm_mssql_database.main.name
}

output "sql_connection_string" {
  description = "The connection string for the SQL Database"
  value       = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  sensitive   = true
}

# Storage Account Outputs
output "storage_account_name" {
  description = "The name of the Storage Account"
  value       = azurerm_storage_account.media.name
}

output "storage_account_primary_endpoint" {
  description = "The primary blob endpoint of the Storage Account"
  value       = azurerm_storage_account.media.primary_blob_endpoint
}

output "storage_account_connection_string" {
  description = "The connection string for the Storage Account"
  value       = azurerm_storage_account.media.primary_connection_string
  sensitive   = true
}

output "media_container_name" {
  description = "The name of the media storage container"
  value       = azurerm_storage_container.media.name
}

# Key Vault Outputs
output "key_vault_name" {
  description = "The name of the Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "The URI of the Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "key_vault_id" {
  description = "The ID of the Key Vault"
  value       = azurerm_key_vault.main.id
}

# Monitoring Outputs
output "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics Workspace"
  value       = azurerm_log_analytics_workspace.main.id
}

output "log_analytics_workspace_name" {
  description = "The name of the Log Analytics Workspace"
  value       = azurerm_log_analytics_workspace.main.name
}

output "application_insights_name" {
  description = "The name of the Application Insights instance"
  value       = azurerm_application_insights.main.name
}

output "application_insights_connection_string" {
  description = "The connection string for Application Insights"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "The instrumentation key for Application Insights"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}

output "application_insights_app_id" {
  description = "The App ID of Application Insights"
  value       = azurerm_application_insights.main.app_id
}

# Monitoring and Alerting Outputs
output "action_group_id" {
  description = "The ID of the monitoring action group"
  value       = azurerm_monitor_action_group.main.id
}

output "action_group_name" {
  description = "The name of the monitoring action group"
  value       = azurerm_monitor_action_group.main.name
}

output "frontend_webtest_id" {
  description = "The ID of the frontend web test"
  value       = azurerm_application_insights_standard_web_test.frontend.id
}

output "api_webtest_id" {
  description = "The ID of the API web test"
  value       = azurerm_application_insights_standard_web_test.api.id
}

# Managed Identity Outputs
output "frontend_app_identity_principal_id" {
  description = "The principal ID of the Frontend App Service managed identity"
  value       = azurerm_linux_web_app.frontend.identity[0].principal_id
}

output "api_app_identity_principal_id" {
  description = "The principal ID of the API App Service managed identity"
  value       = azurerm_linux_web_app.api.identity[0].principal_id
}

output "frontend_app_identity_tenant_id" {
  description = "The tenant ID of the Frontend App Service managed identity"
  value       = azurerm_linux_web_app.frontend.identity[0].tenant_id
}

output "api_app_identity_tenant_id" {
  description = "The tenant ID of the API App Service managed identity"
  value       = azurerm_linux_web_app.api.identity[0].tenant_id
}

# CDN Outputs
output "cdn_profile_name" {
  description = "The name of the CDN Profile"
  value       = azurerm_cdn_profile.main.name
}

output "cdn_frontend_endpoint_url" {
  description = "The URL of the CDN endpoint for the frontend"
  value       = "https://${azurerm_cdn_endpoint.frontend.host_name}"
}

output "cdn_media_endpoint_url" {
  description = "The URL of the CDN endpoint for media files"
  value       = "https://${azurerm_cdn_endpoint.media.host_name}"
}

output "cdn_frontend_endpoint_fqdn" {
  description = "The FQDN of the CDN endpoint for the frontend"
  value       = azurerm_cdn_endpoint.frontend.fqdn
}

output "cdn_media_endpoint_fqdn" {
  description = "The FQDN of the CDN endpoint for media files"
  value       = azurerm_cdn_endpoint.media.fqdn
}

# Application Gateway Outputs
output "application_gateway_public_ip" {
  description = "The public IP address of the Application Gateway"
  value       = var.enable_application_gateway ? azurerm_public_ip.appgw[0].ip_address : null
}

output "application_gateway_fqdn" {
  description = "The FQDN of the Application Gateway"
  value       = var.enable_application_gateway ? azurerm_public_ip.appgw[0].fqdn : null
}

output "application_gateway_id" {
  description = "The ID of the Application Gateway"
  value       = var.enable_application_gateway ? azurerm_application_gateway.main[0].id : null
}

# Virtual Network Outputs
output "vnet_id" {
  description = "The ID of the Virtual Network"
  value       = var.enable_application_gateway ? azurerm_virtual_network.main[0].id : null
}

output "vnet_name" {
  description = "The name of the Virtual Network"
  value       = var.enable_application_gateway ? azurerm_virtual_network.main[0].name : null
}

# Environment Information
output "environment" {
  description = "The deployment environment"
  value       = var.environment
}

output "deployment_timestamp" {
  description = "The timestamp when the infrastructure was deployed"
  value       = timestamp()
}

# Resource Tags
output "common_tags" {
  description = "The common tags applied to all resources"
  value       = var.common_tags
}