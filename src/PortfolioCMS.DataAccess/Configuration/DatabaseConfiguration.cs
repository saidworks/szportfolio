using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Database configuration with Azure SQL Database and Key Vault integration support.
/// Optimized for Azure SQL Database Free tier (32 MB storage limit).
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Adds data access services with EF Core configured for Azure SQL Database.
    /// Connection string can be retrieved from Azure Key Vault using Managed Identity.
    /// </summary>
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        string? connectionStringName = null)
    {
        // Get connection string from configuration
        // Priority: 1) Direct connection string, 2) Key Vault secret, 3) DefaultConnection
        var connectionString = GetConnectionString(configuration, connectionStringName);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Database connection string '{connectionStringName ?? "DefaultConnection"}' not found. " +
                "Ensure it's configured in appsettings.json or Azure Key Vault. " +
                "For Azure Key Vault, configure the Key Vault URI and ensure Managed Identity has access.");
        }

        // Configure Entity Framework with SQL Server/Azure SQL Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            ConfigureSqlServerOptions(options, connectionString);
        });

        // Register repositories using C# 14 collection expressions
        RegisterRepositories(services);

        return services;
    }

    /// <summary>
    /// Retrieves connection string from configuration, supporting Azure Key Vault integration.
    /// </summary>
    private static string? GetConnectionString(IConfiguration configuration, string? connectionStringName)
    {
        var name = connectionStringName ?? "DefaultConnection";
        
        // Try to get from ConnectionStrings section first
        var connectionString = configuration.GetConnectionString(name);
        
        // If not found, try direct configuration key (for Key Vault secrets)
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration[$"SqlConnectionString"];
        }
        
        // Try alternative Key Vault secret name
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration[$"ConnectionStrings:{name}"];
        }
        
        return connectionString;
    }

    /// <summary>
    /// Configures SQL Server options optimized for Azure SQL Database Free tier.
    /// </summary>
    private static void ConfigureSqlServerOptions(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            // Enable connection resiliency for Azure SQL Database
            // Handles transient failures common in cloud environments
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            
            // Configure command timeout (30 seconds default)
            sqlOptions.CommandTimeout(30);
            
            // Set migrations assembly
            sqlOptions.MigrationsAssembly("PortfolioCMS.DataAccess");
            
            // Azure SQL Database Free tier optimizations
            // Limit batch size to reduce memory usage (32 MB limit)
            sqlOptions.MaxBatchSize(42);
            
            // Use memory-optimized settings for small database
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Configure logging and diagnostics based on environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Development")
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        }
        else
        {
            // Production: minimal logging for performance
            options.LogTo(Console.WriteLine, LogLevel.Warning);
        }
        
        // Enable query caching for better performance
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }

    /// <summary>
    /// Registers all repository implementations using C# 14 features.
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services)
    {
        // Register repositories with scoped lifetime
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IMediaFileRepository, MediaFileRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static async Task EnsureDatabaseCreatedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            // Log the exception (implement proper logging)
            throw new InvalidOperationException(
                "Failed to create or migrate database. " +
                "Ensure the connection string is correct and the database server is accessible.", ex);
        }
    }

    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        try
        {
            await Data.DbSeeder.SeedAsync(serviceProvider);
        }
        catch (Exception ex)
        {
            // Log the exception (implement proper logging)
            throw new InvalidOperationException(
                "Failed to seed database. " +
                "Ensure the database is accessible and properly configured.", ex);
        }
    }

}