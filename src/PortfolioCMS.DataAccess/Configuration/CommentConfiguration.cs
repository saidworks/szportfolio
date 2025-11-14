using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Entity configuration for Comment using Fluent API.
/// </summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.AuthorName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");
        
        builder.Property(e => e.AuthorEmail)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)");
        
        builder.Property(e => e.Content)
            .IsRequired()
            .HasMaxLength(2000)
            .HasColumnType("nvarchar(2000)");
        
        builder.Property(e => e.SubmittedDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(e => e.IpAddress)
            .HasMaxLength(45)
            .HasColumnType("nvarchar(45)");
        
        builder.Property(e => e.UserAgent)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Configure indexes
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Comments_Status");
        
        builder.HasIndex(e => e.ArticleId)
            .HasDatabaseName("IX_Comments_ArticleId");
        
        builder.HasIndex(e => e.SubmittedDate)
            .HasDatabaseName("IX_Comments_SubmittedDate");
        
        // Composite index for moderation queries
        builder.HasIndex(e => new { e.Status, e.SubmittedDate })
            .HasDatabaseName("IX_Comments_Status_SubmittedDate");
    }
}
