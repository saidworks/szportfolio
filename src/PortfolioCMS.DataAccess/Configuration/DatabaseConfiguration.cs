using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.DataAccess.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Get connection string from configuration (supports Azure Key Vault)
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' not found. " +
                "Ensure it's configured in appsettings.json or Azure Key Vault.");
        }

        // Configure Entity Framework with database provider detection
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (IsMySqlConnectionString(connectionString))
            {
                // Configure for MySQL (Development)
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    
                    mysqlOptions.CommandTimeout(30);
                    mysqlOptions.MigrationsAssembly("PortfolioCMS.DataAccess");
                });
            }
            else
            {
                // Configure for SQL Server/Azure SQL Database (Staging/Production)
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Enable connection resiliency for Azure SQL Database
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    
                    // Configure command timeout for long-running operations
                    sqlOptions.CommandTimeout(30);
                    
                    // Enable multiple active result sets
                    sqlOptions.MigrationsAssembly("PortfolioCMS.DataAccess");
                });
            }

            // Configure logging and diagnostics
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register repositories
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IMediaFileRepository, MediaFileRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
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

    private static bool IsMySqlConnectionString(string connectionString)
    {
        return connectionString.Contains("Port=3306", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("mysql", StringComparison.OrdinalIgnoreCase);
    }
}