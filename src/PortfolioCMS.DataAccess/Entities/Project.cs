using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PortfolioCMS.DataAccess.Entities;

[Table("Projects")]
[Index(nameof(DisplayOrder))]
[Index(nameof(IsActive))]
public class Project
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(2000)")]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string? TechnologyStack { get; set; }
    
    [MaxLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string? ProjectUrl { get; set; }
    
    [MaxLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string? GitHubUrl { get; set; }
    
    [MaxLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string? ImageUrl { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime? CompletedDate { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Optimistic concurrency control
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
}