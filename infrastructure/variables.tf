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
  description = "The maximum size of the SQL Database in GB (Note: Free tier is 0.032 GB / 32 MB)"
  type        = number
  default     = 2
}

variable "sql_database_sku" {
  description = "The SKU of the SQL Database (Free tier: $0/month, 32 MB storage; Basic: ~$5/month, 2 GB storage)"
  type        = string
  default     = "Free"

  validation {
    condition     = contains(["Free", "Basic", "S0", "S1", "S2", "S3", "P1", "P2", "P4", "P6", "P11", "P15"], var.sql_database_sku)
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

variable "application_insights_retention_days" {
  description = "The retention period for Application Insights data in days"
  type        = number
  default     = 90

  validation {
    condition     = var.application_insights_retention_days >= 30 && var.application_insights_retention_days <= 730
    error_message = "Application Insights retention days must be between 30 and 730."
  }
}

# Alerting Configuration
variable "alert_email_address" {
  description = "Email address for alert notifications"
  type        = string
  default     = "admin@example.com"
}

variable "response_time_threshold_ms" {
  description = "Response time threshold in milliseconds for alerts"
  type        = number
  default     = 3000
}

variable "error_rate_threshold" {
  description = "Error rate threshold for alerts"
  type        = number
  default     = 10
}

variable "dtu_threshold_percent" {
  description = "DTU usage threshold percentage for database alerts"
  type        = number
  default     = 80
}

variable "webtest_geo_locations" {
  description = "Geographic locations for Application Insights web tests"
  type        = list(string)
  default     = ["us-va-ash-azr", "us-il-ch1-azr"]
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

# CDN Configuration
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

variable "cdn_sku" {
  description = "The SKU of the CDN Profile"
  type        = string
  default     = "Standard_Microsoft"

  validation {
    condition     = contains(["Standard_Microsoft", "Standard_Akamai", "Standard_Verizon", "Premium_Verizon"], var.cdn_sku)
    error_message = "CDN SKU must be a valid Azure CDN SKU."
  }
}

# Application Gateway Configuration
variable "enable_application_gateway" {
  description = "Enable Application Gateway with WAF (recommended for production)"
  type        = bool
  default     = false
}

variable "application_gateway_name" {
  description = "The name of the Application Gateway"
  type        = string
  default     = "agw-portfoliocms"
}

variable "application_gateway_sku" {
  description = "The SKU of the Application Gateway"
  type        = string
  default     = "WAF_v2"

  validation {
    condition     = contains(["Standard_v2", "WAF_v2"], var.application_gateway_sku)
    error_message = "Application Gateway SKU must be Standard_v2 or WAF_v2."
  }
}

variable "application_gateway_tier" {
  description = "The tier of the Application Gateway"
  type        = string
  default     = "WAF_v2"

  validation {
    condition     = contains(["Standard_v2", "WAF_v2"], var.application_gateway_tier)
    error_message = "Application Gateway tier must be Standard_v2 or WAF_v2."
  }
}

variable "application_gateway_capacity" {
  description = "The capacity (instance count) of the Application Gateway"
  type        = number
  default     = 2

  validation {
    condition     = var.application_gateway_capacity >= 1 && var.application_gateway_capacity <= 10
    error_message = "Application Gateway capacity must be between 1 and 10."
  }
}

variable "waf_mode" {
  description = "The WAF mode (Detection or Prevention)"
  type        = string
  default     = "Detection"

  validation {
    condition     = contains(["Detection", "Prevention"], var.waf_mode)
    error_message = "WAF mode must be Detection or Prevention."
  }
}

variable "ssl_certificate_data" {
  description = "The base64-encoded SSL certificate data (PFX format)"
  type        = string
  default     = ""
  sensitive   = true
}

variable "ssl_certificate_password" {
  description = "The password for the SSL certificate"
  type        = string
  default     = ""
  sensitive   = true
}

# Virtual Network Configuration
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

# Subnet Configuration
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

variable "appgw_subnet_address_prefixes" {
  description = "The address prefixes for the Application Gateway subnet"
  type        = list(string)
  default     = ["10.0.2.0/24"]
}