# Azure Application Gateway WAF Policy Configuration
# This module defines Web Application Firewall rules and policies for protecting the application

resource "azurerm_web_application_firewall_policy" "main" {
  name                = var.waf_policy_name
  resource_group_name = var.resource_group_name
  location            = var.location

  # Custom rules for application-specific protection
  custom_rules {
    name      = "RateLimitRule"
    priority  = 1
    rule_type = "RateLimitRule"

    match_conditions {
      match_variables {
        variable_name = "RemoteAddr"
      }

      operator           = "IPMatch"
      negation_condition = false
      match_values       = ["0.0.0.0/0"]
    }

    action = "Block"
    rate_limit_duration = "OneMin"
    rate_limit_threshold = 100
  }

  custom_rules {
    name      = "BlockSuspiciousUserAgents"
    priority  = 2
    rule_type = "MatchRule"

    match_conditions {
      match_variables {
        variable_name = "RequestHeaders"
        selector      = "User-Agent"
      }

      operator           = "Contains"
      negation_condition = false
      match_values = [
        "sqlmap",
        "nikto",
        "nmap",
        "masscan",
        "nessus"
      ]
    }

    action = "Block"
  }

  custom_rules {
    name      = "BlockKnownBadIPs"
    priority  = 3
    rule_type = "MatchRule"

    match_conditions {
      match_variables {
        variable_name = "RemoteAddr"
      }

      operator           = "IPMatch"
      negation_condition = false
      match_values       = var.blocked_ip_addresses
    }

    action = "Block"
  }

  custom_rules {
    name      = "AllowOnlySpecificCountries"
    priority  = 4
    rule_type = "MatchRule"

    match_conditions {
      match_variables {
        variable_name = "RemoteAddr"
      }

      operator           = "GeoMatch"
      negation_condition = true
      match_values       = var.allowed_countries
    }

    action = "Block"
  }

  # Policy settings
  policy_settings {
    enabled                     = true
    mode                        = var.waf_mode
    request_body_check          = true
    file_upload_limit_in_mb     = 100
    max_request_body_size_in_kb = 128
  }

  # Managed rules (OWASP Core Rule Set)
  managed_rules {
    managed_rule_set {
      type    = "OWASP"
      version = "3.2"

      # Disable specific rules that may cause false positives
      rule_group_override {
        rule_group_name = "REQUEST-942-APPLICATION-ATTACK-SQLI"

        rule {
          id      = "942100"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "942200"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "942260"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "942340"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "942370"
          enabled = true
          action  = "Block"
        }
      }

      rule_group_override {
        rule_group_name = "REQUEST-931-APPLICATION-ATTACK-RFI"

        rule {
          id      = "931100"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "931110"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "931120"
          enabled = true
          action  = "Block"
        }
      }

      rule_group_override {
        rule_group_name = "REQUEST-941-APPLICATION-ATTACK-XSS"

        rule {
          id      = "941100"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "941110"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "941130"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "941180"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "941320"
          enabled = true
          action  = "Block"
        }
      }

      rule_group_override {
        rule_group_name = "REQUEST-920-PROTOCOL-ENFORCEMENT"

        rule {
          id      = "920300"
          enabled = true
          action  = "Block"
        }

        rule {
          id      = "920320"
          enabled = true
          action  = "Block"
        }
      }
    }

    managed_rule_set {
      type    = "Microsoft_BotManagerRuleSet"
      version = "1.0"
    }
  }

  tags = var.common_tags
}
