# Grafana Cloud Integration Setup

This document provides instructions for integrating Azure Application Insights with Grafana Cloud for enhanced monitoring and visualization.

## Prerequisites

1. **Grafana Cloud Account** - Sign up at https://grafana.com/products/cloud/
2. **Azure Application Insights** - Deployed via Terraform (already configured)
3. **Azure Service Principal** - For Grafana to access Azure resources

## Step 1: Create Azure Service Principal for Grafana

```bash
# Create a service principal for Grafana Cloud
az ad sp create-for-rbac --name "grafana-cloud-reader" \
  --role "Monitoring Reader" \
  --scopes /subscriptions/<your-subscription-id>

# Output will include:
# - appId (Client ID)
# - password (Client Secret)
# - tenant (Tenant ID)
```

## Step 2: Configure Azure Monitor Data Source in Grafana Cloud

1. Log in to your Grafana Cloud instance
2. Navigate to **Configuration** → **Data Sources**
3. Click **Add data source**
4. Select **Azure Monitor**
5. Configure the following settings:

### Authentication Settings
- **Authentication**: Service Principal
- **Directory (tenant) ID**: Your Azure AD tenant ID
- **Application (client) ID**: Service principal appId from Step 1
- **Client Secret**: Service principal password from Step 1
- **Default Subscription**: Your Azure subscription ID

### Resource Settings
- **Default Resource Group**: `rg-portfoliocms-{environment}`
- **Default Region**: `East US` (or your configured region)

6. Click **Save & Test** to verify the connection

## Step 3: Import Pre-built Dashboards

### Application Performance Dashboard

Create a new dashboard with the following panels:

#### Panel 1: Request Rate
```kusto
requests
| where timestamp > ago(1h)
| summarize RequestCount = count() by bin(timestamp, 5m)
| render timechart
```

#### Panel 2: Response Time
```kusto
requests
| where timestamp > ago(1h)
| summarize AvgDuration = avg(duration), P95Duration = percentile(duration, 95) by bin(timestamp, 5m)
| render timechart
```

#### Panel 3: Error Rate
```kusto
requests
| where timestamp > ago(1h)
| summarize TotalRequests = count(), FailedRequests = countif(success == false) by bin(timestamp, 5m)
| extend ErrorRate = (FailedRequests * 100.0) / TotalRequests
| render timechart
```

#### Panel 4: Top 10 Slowest Requests
```kusto
requests
| where timestamp > ago(1h)
| top 10 by duration desc
| project timestamp, name, url, duration, resultCode
```

### Infrastructure Health Dashboard

#### Panel 1: Database DTU Usage
```kusto
AzureMetrics
| where ResourceProvider == "MICROSOFT.SQL"
| where MetricName == "dtu_consumption_percent"
| summarize AvgDTU = avg(Average) by bin(TimeGenerated, 5m)
| render timechart
```

#### Panel 2: App Service CPU Usage
```kusto
AzureMetrics
| where ResourceProvider == "MICROSOFT.WEB"
| where MetricName == "CpuPercentage"
| summarize AvgCPU = avg(Average) by Resource, bin(TimeGenerated, 5m)
| render timechart
```

#### Panel 3: Storage Account Availability
```kusto
AzureMetrics
| where ResourceProvider == "MICROSOFT.STORAGE"
| where MetricName == "Availability"
| summarize AvgAvailability = avg(Average) by bin(TimeGenerated, 5m)
| render timechart
```

### User Experience Dashboard

#### Panel 1: Page Views
```kusto
pageViews
| where timestamp > ago(1h)
| summarize PageViews = count() by bin(timestamp, 5m)
| render timechart
```

#### Panel 2: User Sessions
```kusto
pageViews
| where timestamp > ago(1h)
| summarize UniqueUsers = dcount(user_Id) by bin(timestamp, 5m)
| render timechart
```

#### Panel 3: Browser Performance
```kusto
browserTimings
| where timestamp > ago(1h)
| summarize AvgLoadTime = avg(totalDuration) by bin(timestamp, 5m)
| render timechart
```

## Step 4: Configure Alerting Rules in Grafana Cloud

### Alert 1: High Response Time

1. Navigate to **Alerting** → **Alert rules**
2. Click **New alert rule**
3. Configure:
   - **Name**: High Response Time
   - **Query**: 
     ```kusto
     requests
     | where timestamp > ago(5m)
     | summarize AvgDuration = avg(duration)
     ```
   - **Condition**: `AvgDuration > 3000` (3 seconds)
   - **Evaluation**: Every 5 minutes
   - **For**: 10 minutes

### Alert 2: High Error Rate

1. Create new alert rule
2. Configure:
   - **Name**: High Error Rate
   - **Query**:
     ```kusto
     requests
     | where timestamp > ago(5m)
     | summarize ErrorRate = (countif(success == false) * 100.0) / count()
     ```
   - **Condition**: `ErrorRate > 5` (5%)
   - **Evaluation**: Every 5 minutes
   - **For**: 5 minutes

### Alert 3: Database High DTU Usage

1. Create new alert rule
2. Configure:
   - **Name**: Database High DTU Usage
   - **Query**:
     ```kusto
     AzureMetrics
     | where ResourceProvider == "MICROSOFT.SQL"
     | where MetricName == "dtu_consumption_percent"
     | summarize AvgDTU = avg(Average)
     ```
   - **Condition**: `AvgDTU > 80` (80%)
   - **Evaluation**: Every 5 minutes
   - **For**: 10 minutes

### Alert 4: Application Availability

1. Create new alert rule
2. Configure:
   - **Name**: Application Availability
   - **Query**:
     ```kusto
     availabilityResults
     | where timestamp > ago(5m)
     | summarize AvailabilityPercent = (countif(success == true) * 100.0) / count()
     ```
   - **Condition**: `AvailabilityPercent < 95` (95%)
   - **Evaluation**: Every 5 minutes
   - **For**: 5 minutes

## Step 5: Configure Notification Channels

### Email Notifications

1. Navigate to **Alerting** → **Contact points**
2. Click **New contact point**
3. Configure:
   - **Name**: Email Alerts
   - **Type**: Email
   - **Addresses**: Your alert email addresses
   - **Subject**: `[{{ .Status }}] {{ .GroupLabels.alertname }}`
   - **Message**: Custom alert template

### Slack Notifications (Optional)

1. Create new contact point
2. Configure:
   - **Name**: Slack Alerts
   - **Type**: Slack
   - **Webhook URL**: Your Slack webhook URL
   - **Channel**: `#alerts` or your preferred channel

### PagerDuty Integration (Optional)

1. Create new contact point
2. Configure:
   - **Name**: PagerDuty
   - **Type**: PagerDuty
   - **Integration Key**: Your PagerDuty integration key

## Step 6: Create Notification Policies

1. Navigate to **Alerting** → **Notification policies**
2. Configure routing:
   - **Critical alerts** (severity: critical) → PagerDuty + Email
   - **Warning alerts** (severity: warning) → Email + Slack
   - **Info alerts** (severity: info) → Slack only

## Step 7: Export Application Insights Data to Grafana Cloud

For long-term storage and advanced analytics, configure continuous export:

```bash
# Create a storage account for continuous export (if not already exists)
az storage account create \
  --name stportfoliocmslogs \
  --resource-group rg-portfoliocms-prod \
  --location eastus \
  --sku Standard_LRS

# Get the storage account connection string
az storage account show-connection-string \
  --name stportfoliocmslogs \
  --resource-group rg-portfoliocms-prod

# Configure continuous export in Application Insights
# This must be done via Azure Portal:
# 1. Navigate to Application Insights → Configure → Continuous Export
# 2. Add export destination (Storage Account)
# 3. Select telemetry types to export
```

## Step 8: Custom Metrics Integration

To send custom metrics from your application to Grafana Cloud:

### In Your Application Code

```csharp
// Program.cs or Startup.cs
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public class CustomMetricsService
{
    private readonly TelemetryClient _telemetryClient;

    public CustomMetricsService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackBusinessMetric(string metricName, double value, Dictionary<string, string> properties = null)
    {
        _telemetryClient.TrackMetric(metricName, value, properties);
    }
}

// Example usage
public class ArticleService
{
    private readonly CustomMetricsService _metrics;

    public async Task<Article> CreateArticle(CreateArticleDto dto)
    {
        var article = await _repository.CreateAsync(dto);
        
        // Track custom business metric
        _metrics.TrackBusinessMetric("articles.created", 1, new Dictionary<string, string>
        {
            ["category"] = article.Category,
            ["author"] = article.AuthorId
        });
        
        return article;
    }
}
```

### Query Custom Metrics in Grafana

```kusto
customMetrics
| where name == "articles.created"
| where timestamp > ago(24h)
| summarize ArticlesCreated = sum(value) by bin(timestamp, 1h)
| render timechart
```

## Step 9: Dashboard Templates

Import these dashboard JSON templates into Grafana Cloud:

### Template 1: Application Overview
- Request rate, response time, error rate
- Active users, page views
- Top endpoints by traffic and latency

### Template 2: Infrastructure Health
- CPU, memory, disk usage
- Database performance metrics
- Storage account metrics

### Template 3: Business Metrics
- Articles published per day
- Comments submitted per day
- User engagement metrics

## Step 10: Verify Integration

1. Check that metrics are flowing from Azure to Grafana
2. Verify dashboards are displaying data correctly
3. Test alert rules by triggering conditions
4. Confirm notifications are being sent

## Troubleshooting

### No Data in Grafana

1. Verify service principal has correct permissions
2. Check Application Insights is collecting data
3. Verify time range in Grafana queries
4. Check Azure Monitor data source configuration

### Alerts Not Firing

1. Verify alert rule conditions are correct
2. Check evaluation frequency and duration
3. Verify notification channels are configured
4. Check Grafana Cloud logs for errors

### High Costs

1. Review Application Insights data retention settings
2. Optimize query frequency in dashboards
3. Use sampling for high-volume telemetry
4. Consider using Grafana Cloud's free tier limits

## Best Practices

1. **Use Variables**: Create dashboard variables for environment, resource group, etc.
2. **Set Appropriate Retention**: Balance cost vs. data retention needs
3. **Use Folders**: Organize dashboards by category (App, Infrastructure, Business)
4. **Document Queries**: Add descriptions to panels explaining what they show
5. **Test Alerts**: Regularly test alert rules to ensure they work
6. **Review Regularly**: Review and update dashboards and alerts monthly

## Additional Resources

- [Grafana Cloud Documentation](https://grafana.com/docs/grafana-cloud/)
- [Azure Monitor Data Source](https://grafana.com/docs/grafana/latest/datasources/azuremonitor/)
- [Application Insights Query Language](https://docs.microsoft.com/en-us/azure/azure-monitor/logs/get-started-queries)
- [Grafana Alerting](https://grafana.com/docs/grafana/latest/alerting/)

## Cost Estimation

### Grafana Cloud Free Tier
- 10,000 series for Prometheus metrics
- 50 GB logs
- 50 GB traces
- 3 users

### Grafana Cloud Pro (if needed)
- ~$49/month base
- Additional costs for metrics, logs, traces beyond free tier

### Azure Application Insights
- First 5 GB/month: Free
- Additional data: $2.30/GB
- Data retention beyond 90 days: $0.10/GB/month

**Estimated Monthly Cost**: $0-20 for development, $20-50 for production (depending on traffic)
