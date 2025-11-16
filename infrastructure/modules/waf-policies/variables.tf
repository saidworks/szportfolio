# WAF Policy Variables

variable "waf_policy_name" {
  description = "The name of the WAF policy"
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where the WAF policy will be created"
  type        = string
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

variable "blocked_ip_addresses" {
  description = "List of IP addresses to block"
  type        = list(string)
  default     = []
}

variable "allowed_countries" {
  description = "List of country codes allowed to access the application (ISO 3166-1 alpha-2)"
  type        = list(string)
  default     = ["US", "CA", "GB", "DE", "FR", "AU", "NZ", "JP", "SG"]
}

variable "common_tags" {
  description = "Common tags to be applied to all resources"
  type        = map(string)
  default     = {}
}
