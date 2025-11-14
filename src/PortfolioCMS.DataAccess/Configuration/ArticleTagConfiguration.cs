using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Entity configuration for ArticleTag junction table using Fluent API.
/// </summary>
public class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTag>
{
    public void Configure(EntityTypeBuilder<ArticleTag> builder)
    {
        builder.ToTable("ArticleTags");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.CreatedDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure indexes
        builder.HasIndex(e => e.ArticleId)
            .HasDatabaseName("IX_ArticleTags_ArticleId");
        
        builder.HasIndex(e => e.TagId)
            .HasDatabaseName("IX_ArticleTags_TagId");
        
        // Unique constraint to prevent duplicate article-tag associations
        builder.HasIndex(e => new { e.ArticleId, e.TagId })
            .IsUnique()
            .HasDatabaseName("IX_ArticleTags_ArticleId_TagId_Unique");

        // Configure relationships
        builder.HasOne(e => e.Article)
            .WithMany(a => a.ArticleTags)
            .HasForeignKey(e => e.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Tag)
            .WithMany(t => t.ArticleTags)
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
