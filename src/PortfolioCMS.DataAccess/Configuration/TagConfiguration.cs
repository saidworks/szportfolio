using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Entity configuration for Tag using Fluent API.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("nvarchar(50)");
        
        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("nvarchar(50)");
        
        builder.Property(e => e.Description)
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)");
        
        builder.Property(e => e.CreatedDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure indexes
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Tags_Slug_Unique");
        
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Tags_Name");
    }
}
