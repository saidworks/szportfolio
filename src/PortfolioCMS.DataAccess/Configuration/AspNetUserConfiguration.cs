using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Configuration;

/// <summary>
/// Entity configuration for AspNetUser extensions using Fluent API.
/// </summary>
public class AspNetUserConfiguration : IEntityTypeConfiguration<AspNetUser>
{
    public void Configure(EntityTypeBuilder<AspNetUser> builder)
    {
        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");
        
        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");
        
        builder.Property(e => e.CreatedDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.Property(e => e.LastLoginDate)
            .HasColumnType("datetime2");
        
        builder.Property(e => e.Role)
            .HasMaxLength(50)
            .HasColumnType("nvarchar(50)")
            .HasDefaultValue("Viewer");

        // Configure indexes
        builder.HasIndex(e => e.Email)
            .HasDatabaseName("IX_AspNetUsers_Email");
        
        builder.HasIndex(e => e.CreatedDate)
            .HasDatabaseName("IX_AspNetUsers_CreatedDate");
        
        builder.HasIndex(e => e.Role)
            .HasDatabaseName("IX_AspNetUsers_Role");
    }
}
