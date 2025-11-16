# WAF Policy Outputs

output "waf_policy_id" {
  description = "The ID of the WAF policy"
  value       = azurerm_web_application_firewall_policy.main.id
}

output "waf_policy_name" {
  description = "The name of the WAF policy"
  value       = azurerm_web_application_firewall_policy.main.name
}

output "waf_policy_mode" {
  description = "The mode of the WAF policy"
  value       = azurerm_web_application_firewall_policy.main.policy_settings[0].mode
}
