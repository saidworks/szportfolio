using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Entity configuration for Project using Fluent API.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)");
        
        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000)
            .HasColumnType("nvarchar(2000)");
        
        builder.Property(e => e.TechnologyStack)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");
        
        builder.Property(e => e.ProjectUrl)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");
        
        builder.Property(e => e.GitHubUrl)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");
        
        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");
        
        builder.Property(e => e.CompletedDate)
            .HasColumnType("datetime2");
        
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Configure indexes
        builder.HasIndex(e => e.DisplayOrder)
            .HasDatabaseName("IX_Projects_DisplayOrder");
        
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_Projects_IsActive");
        
        // Composite index for active projects ordered by display order
        builder.HasIndex(e => new { e.IsActive, e.DisplayOrder })
            .HasDatabaseName("IX_Projects_IsActive_DisplayOrder");

        // Configure relationships
        builder.HasMany(e => e.MediaFiles)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
