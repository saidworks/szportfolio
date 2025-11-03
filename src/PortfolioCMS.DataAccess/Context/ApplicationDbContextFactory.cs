using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PortfolioCMS.DataAccess.Context;

/// <summary>
/// Design-time factory for ApplicationDbContext to enable EF Core migrations
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for design-time operations (SQL Server)
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=PortfolioCMS;Trusted_Connection=true;MultipleActiveResultSets=true";
        }

        // Configure DbContext options based on connection string
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        if (IsMySqlConnectionString(connectionString))
        {
            // Configure for MySQL (Development)
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
            {
                mysqlOptions.MigrationsAssembly("PortfolioCMS.DataAccess");
                mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }
        else
        {
            // Configure for SQL Server (Staging/Production)
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("PortfolioCMS.DataAccess");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static bool IsMySqlConnectionString(string connectionString)
    {
        return connectionString.Contains("Port=3306", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("mysql", StringComparison.OrdinalIgnoreCase);
    }
}