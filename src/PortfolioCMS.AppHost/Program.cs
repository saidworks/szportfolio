var builder = DistributedApplication.CreateBuilder(args);

// Add Azure SQL Database resource
var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sqlDatabase = builder.AddSqlServer("sql", sqlPassword)
    .WithDataVolume()
    .AddDatabase("portfoliodb");

// Add Azure Key Vault resource (for production)
var keyVault = builder.AddAzureKeyVault("keyvault");

// Add API project with service discovery
var apiService = builder.AddProject<Projects.PortfolioCMS_API>("api")
    .WithReference(sqlDatabase)
    .WithReference(keyVault)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

// Add Frontend project with service discovery to API
var frontend = builder.AddProject<Projects.PortfolioCMS_Frontend>("frontend")
    .WithReference(apiService)  // Enables service discovery
    .WithExternalHttpEndpoints();

builder.Build().Run();
