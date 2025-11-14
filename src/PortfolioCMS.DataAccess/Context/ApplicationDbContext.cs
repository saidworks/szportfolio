using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Context;

/// <summary>
/// Application database context using EF Core 10 with C# 14 primary constructor.
/// Configured for Azure SQL Database Free tier with optimizations.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<AspNetUser>(options)
{
    // DbSets for all entities - using C# 14 collection expressions where applicable
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations using C# 14 collection expressions
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure many-to-many relationship between Article and Tag
        builder.Entity<Article>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.Articles)
            .UsingEntity<ArticleTag>();

        // Configure Azure SQL Database specific optimizations
        ConfigureAzureSqlOptimizations(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will be overridden by dependency injection configuration
            // but provides a fallback for design-time operations (migrations, etc.)
            optionsBuilder.UseSqlServer();
        }

        // Configure based on environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }

    /// <summary>
    /// Configures Azure SQL Database specific optimizations for Free tier (32 MB limit).
    /// </summary>
    private static void ConfigureAzureSqlOptimizations(ModelBuilder builder)
    {
        // Set default schema
        builder.HasDefaultSchema("dbo");

        // Configure string comparison to use case-insensitive collation (Azure SQL default)
        // This improves query performance for string comparisons
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    // Use SQL_Latin1_General_CP1_CI_AS for case-insensitive comparisons
                    property.SetCollation("SQL_Latin1_General_CP1_CI_AS");
                }
            }
        }

        // Configure temporal tables for audit trails (optional - can be enabled later)
        // Temporal tables provide automatic history tracking
        // builder.Entity<Article>().ToTable(tb => tb.IsTemporal());
        // builder.Entity<Comment>().ToTable(tb => tb.IsTemporal());
        // builder.Entity<Project>().ToTable(tb => tb.IsTemporal());

        // Note: Full-text search can be configured via migrations for advanced search
        // This requires creating full-text catalogs and indexes in SQL Server
    }
}