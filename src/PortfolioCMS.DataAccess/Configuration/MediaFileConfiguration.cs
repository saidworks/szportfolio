using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Entity configuration for MediaFile using Fluent API.
/// </summary>
public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("nvarchar(255)");
        
        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("nvarchar(255)");
        
        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");
        
        builder.Property(e => e.BlobUrl)
            .IsRequired()
            .HasMaxLength(1000)
            .HasColumnType("nvarchar(1000)");
        
        builder.Property(e => e.Category)
            .HasMaxLength(50)
            .HasColumnType("nvarchar(50)");
        
        builder.Property(e => e.UploadedDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.Property(e => e.UploadedBy)
            .HasMaxLength(450)
            .HasColumnType("nvarchar(450)");

        // Configure indexes
        builder.HasIndex(e => e.Category)
            .HasDatabaseName("IX_MediaFiles_Category");
        
        builder.HasIndex(e => e.UploadedDate)
            .HasDatabaseName("IX_MediaFiles_UploadedDate");
        
        builder.HasIndex(e => e.UploadedBy)
            .HasDatabaseName("IX_MediaFiles_UploadedBy");
        
        // Composite index for filtering by category and date
        builder.HasIndex(e => new { e.Category, e.UploadedDate })
            .HasDatabaseName("IX_MediaFiles_Category_UploadedDate");

        // Configure relationships
        builder.HasOne(e => e.UploadedByUser)
            .WithMany(u => u.UploadedMediaFiles)
            .HasForeignKey(e => e.UploadedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
