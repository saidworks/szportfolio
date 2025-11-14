using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Entity configuration for Article using Fluent API.
/// Optimized for Azure SQL Database Free tier with proper indexing.
/// </summary>
public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(e => e.Id);
        
        // Configure properties with Azure SQL optimizations
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)");
        
        builder.Property(e => e.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
        
        builder.Property(e => e.Summary)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");
        
        builder.Property(e => e.CreatedDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.Property(e => e.PublishedDate)
            .HasColumnType("datetime2");
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(e => e.FeaturedImageUrl)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");
        
        builder.Property(e => e.MetaDescription)
            .HasMaxLength(300)
            .HasColumnType("nvarchar(300)");
        
        builder.Property(e => e.MetaKeywords)
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)");
        
        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .HasColumnType("nvarchar(450)");
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Configure indexes for performance optimization
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Articles_Status");
        
        builder.HasIndex(e => e.PublishedDate)
            .HasDatabaseName("IX_Articles_PublishedDate");
        
        builder.HasIndex(e => e.CreatedDate)
            .HasDatabaseName("IX_Articles_CreatedDate");
        
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_Articles_UserId");
        
        // Composite index for common queries (status + published date)
        builder.HasIndex(e => new { e.Status, e.PublishedDate })
            .HasDatabaseName("IX_Articles_Status_PublishedDate");

        // Configure relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.Articles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Article)
            .HasForeignKey(c => c.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.MediaFiles)
            .WithOne(m => m.Article)
            .HasForeignKey(m => m.ArticleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
