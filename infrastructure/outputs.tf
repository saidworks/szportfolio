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