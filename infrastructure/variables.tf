# Environment Configuration
variable "environment" {
  description = "The deployment environment (development, staging, production)"
  type        = string
  default     = "development"
  
  validation {
    condition     = contains(["development", "staging", "production"], var.environment)
    error_message = "Environment must be one of: development, staging, production."
  }
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
  default     = "East US"
}

# Resource Group
variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
  default     = "rg-portfoliocms"
}

# App Service Configuration
variable "app_service_plan_name" {
  description = "The name of the App Service Plan"
  type        = string
  default     = "asp-portfoliocms"
}

variable "app_service_plan_sku" {
  description = "The SKU of the App Service Plan"
  type        = string
  default     = "B1"
  
  validation {
    condition     = contains(["B1", "B2", "B3", "S1", "S2", "S3", "P1v2", "P2v2", "P3v2"], var.app_service_plan_sku)
    error_message = "App Service Plan SKU must be a valid Azure App Service Plan SKU."
  }
}

variable "frontend_app_name" {
  description = "The name of the Frontend App Service"
  type        = string
  default     = "app-portfoliocms-frontend"
}

variable "api_app_name" {
  description = "The name of the API App Service"
  type        = string
  default     = "app-portfoliocms-api"
}

# SQL Database Configuration
variable "sql_server_name" {
  description = "The name of the SQL Server"
  type        = string
  default     = "sql-portfoliocms"
}

variable "sql_database_name" {
  description = "The name of the SQL Database"
  type        = string
  default     = "sqldb-portfoliocms"
}

variable "sql_admin_username" {
  description = "The administrator username for the SQL Server"
  type        = string
  default     = "sqladmin"
  sensitive   = true
}

variable "sql_admin_password" {
  description = "The administrator password for the SQL Server"
  type        = string
  sensitive   = true
  
  validation {
    condition     = length(var.sql_admin_password) >= 8
    error_message = "SQL admin password must be at least 8 characters long."
  }
}

variable "sql_database_max_size_gb" {
  description = "The maximum size of the SQL Database in GB"
  type        = number
  default     = 2
}

variable "sql_database_sku" {
  description = "The SKU of the SQL Database"
  type        = string
  default     = "Basic"
  
  validation {
    condition     = contains(["Basic", "S0", "S1", "S2", "S3", "P1", "P2", "P4", "P6", "P11", "P15"], var.sql_database_sku)
    error_message = "SQL Database SKU must be a valid Azure SQL Database SKU."
  }
}

# Storage Account Configuration
variable "storage_account_name" {
  description = "The name of the Storage Account for media files"
  type        = string
  default     = "stportfoliocmsmedia"
  
  validation {
    condition     = length(var.storage_account_name) >= 3 && length(var.storage_account_name) <= 24 && can(regex("^[a-z0-9]+$", var.storage_account_name))
    error_message = "Storage account name must be between 3 and 24 characters long and contain only lowercase letters and numbers."
  }
}

# Key Vault Configuration
variable "key_vault_name" {
  description = "The name of the Key Vault"
  type        = string
  default     = "kv-portfoliocms"
}

# Monitoring Configuration
variable "log_analytics_workspace_name" {
  description = "The name of the Log Analytics Workspace"
  type        = string
  default     = "law-portfoliocms"
}

variable "log_analytics_retention_days" {
  description = "The retention period for Log Analytics in days"
  type        = number
  default     = 30
  
  validation {
    condition     = var.log_analytics_retention_days >= 30 && var.log_analytics_retention_days <= 730
    error_message = "Log Analytics retention days must be between 30 and 730."
  }
}

variable "application_insights_name" {
  description = "The name of the Application Insights instance"
  type        = string
  default     = "ai-portfoliocms"
}

# Common Tags
variable "common_tags" {
  description = "Common tags to be applied to all resources"
  type        = map(string)
  default = {
    Project     = "PortfolioCMS"
    Environment = "development"
    ManagedBy   = "Terraform"
    Owner       = "DevOps Team"
  }
}

# CDN Configuration (for future use)
variable "cdn_profile_name" {
  description = "The name of the CDN Profile"
  type        = string
  default     = "cdn-portfoliocms"
}

variable "cdn_endpoint_name" {
  description = "The name of the CDN Endpoint"
  type        = string
  default     = "cdne-portfoliocms"
}

# Application Gateway Configuration (for future use)
variable "application_gateway_name" {
  description = "The name of the Application Gateway"
  type        = string
  default     = "agw-portfoliocms"
}

# Virtual Network Configuration (for future use)
variable "vnet_name" {
  description = "The name of the Virtual Network"
  type        = string
  default     = "vnet-portfoliocms"
}

variable "vnet_address_space" {
  description = "The address space for the Virtual Network"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

# Subnet Configuration (for future use)
variable "subnet_name" {
  description = "The name of the subnet"
  type        = string
  default     = "snet-portfoliocms"
}

variable "subnet_address_prefixes" {
  description = "The address prefixes for the subnet"
  type        = list(string)
  default     = ["10.0.1.0/24"]
}