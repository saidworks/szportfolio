using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Context;

public class ApplicationDbContext : IdentityDbContext<AspNetUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<Article> Articles { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Article entity
        builder.Entity<Article>(entity =>
        {
            entity.ToTable("Articles");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");
            
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            
            entity.Property(e => e.Summary)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.PublishedDate)
                .HasColumnType("datetime2");
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
            
            entity.Property(e => e.FeaturedImageUrl)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            
            entity.Property(e => e.MetaDescription)
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");
            
            entity.Property(e => e.MetaKeywords)
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");
            
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnType("nvarchar(450)");
            
            entity.Property(e => e.RowVersion)
                .IsRowVersion();

            // Indexes for performance optimization
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Articles_Status");
            
            entity.HasIndex(e => e.PublishedDate)
                .HasDatabaseName("IX_Articles_PublishedDate");
            
            entity.HasIndex(e => e.CreatedDate)
                .HasDatabaseName("IX_Articles_CreatedDate");
            
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Articles_UserId");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.Articles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Comment entity
        builder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AuthorName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");
            
            entity.Property(e => e.AuthorEmail)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");
            
            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("nvarchar(2000)");
            
            entity.Property(e => e.SubmittedDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
            
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnType("nvarchar(45)");
            
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            
            entity.Property(e => e.RowVersion)
                .IsRowVersion();

            // Indexes for performance optimization
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Comments_Status");
            
            entity.HasIndex(e => e.ArticleId)
                .HasDatabaseName("IX_Comments_ArticleId");
            
            entity.HasIndex(e => e.SubmittedDate)
                .HasDatabaseName("IX_Comments_SubmittedDate");

            // Relationships
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Tag entity
        builder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");
            
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");
            
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance optimization
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Tags_Slug_Unique");
            
            entity.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Tags_Name");
        });

        // Configure ArticleTag junction entity
        builder.Entity<ArticleTag>(entity =>
        {
            entity.ToTable("ArticleTags");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance optimization
            entity.HasIndex(e => e.ArticleId)
                .HasDatabaseName("IX_ArticleTags_ArticleId");
            
            entity.HasIndex(e => e.TagId)
                .HasDatabaseName("IX_ArticleTags_TagId");
            
            entity.HasIndex(e => new { e.ArticleId, e.TagId })
                .IsUnique()
                .HasDatabaseName("IX_ArticleTags_ArticleId_TagId_Unique");

            // Relationships
            entity.HasOne(e => e.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure many-to-many relationship between Article and Tag
        builder.Entity<Article>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.Articles)
            .UsingEntity<ArticleTag>();

        // Configure Project entity
        builder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");
            
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("nvarchar(2000)");
            
            entity.Property(e => e.TechnologyStack)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            
            entity.Property(e => e.ProjectUrl)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            
            entity.Property(e => e.GitHubUrl)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            
            entity.Property(e => e.CompletedDate)
                .HasColumnType("datetime2");
            
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
            
            entity.Property(e => e.RowVersion)
                .IsRowVersion();

            // Indexes for performance optimization
            entity.HasIndex(e => e.DisplayOrder)
                .HasDatabaseName("IX_Projects_DisplayOrder");
            
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Projects_IsActive");
        });

        // Configure MediaFile entity
        builder.Entity<MediaFile>(entity =>
        {
            entity.ToTable("MediaFiles");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");
            
            entity.Property(e => e.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");
            
            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");
            
            entity.Property(e => e.BlobUrl)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");
            
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");
            
            entity.Property(e => e.UploadedDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.UploadedBy)
                .HasMaxLength(450)
                .HasColumnType("nvarchar(450)");

            // Indexes for performance optimization
            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_MediaFiles_Category");
            
            entity.HasIndex(e => e.UploadedDate)
                .HasDatabaseName("IX_MediaFiles_UploadedDate");
            
            entity.HasIndex(e => e.UploadedBy)
                .HasDatabaseName("IX_MediaFiles_UploadedBy");

            // Relationships
            entity.HasOne(e => e.Article)
                .WithMany(a => a.MediaFiles)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.MediaFiles)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.UploadedByUser)
                .WithMany(u => u.UploadedMediaFiles)
                .HasForeignKey(e => e.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure AspNetUser entity extensions
        builder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");
            
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");
            
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.LastLoginDate)
                .HasColumnType("datetime2");
            
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                .HasDefaultValue("Viewer");

            // Indexes for performance optimization
            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_AspNetUsers_Email");
            
            entity.HasIndex(e => e.CreatedDate)
                .HasDatabaseName("IX_AspNetUsers_CreatedDate");
        });

        // Configure Azure SQL Database specific optimizations
        ConfigureAzureSqlOptimizations(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will be overridden by dependency injection configuration
            // but provides a fallback for design-time operations
            optionsBuilder.UseSqlServer();
        }

        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        // Enable detailed errors in development
        optionsBuilder.EnableDetailedErrors();
    }

    private static void ConfigureAzureSqlOptimizations(ModelBuilder builder)
    {
        // Configure connection resiliency for Azure SQL Database
        // This will be handled in the service configuration

        // Set default schema
        builder.HasDefaultSchema("dbo");

        // Configure temporal tables for audit trails (can be enabled later)
        // builder.Entity<Article>().ToTable(tb => tb.IsTemporal());
        // builder.Entity<Comment>().ToTable(tb => tb.IsTemporal());

        // Configure computed columns for full-text search
        // This can be added later for advanced search functionality
    }
}