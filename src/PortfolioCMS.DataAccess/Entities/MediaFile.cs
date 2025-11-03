using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PortfolioCMS.DataAccess.Entities;

[Table("MediaFiles")]
[Index(nameof(Category))]
[Index(nameof(UploadedDate))]
[Index(nameof(UploadedBy))]
public class MediaFile
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Column(TypeName = "nvarchar(255)")]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    [Column(TypeName = "nvarchar(255)")]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(1000)]
    [Column(TypeName = "nvarchar(1000)")]
    public string BlobUrl { get; set; } = string.Empty;
    
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string Category { get; set; } = string.Empty;
    
    [Column(TypeName = "datetime2")]
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(450)]
    [Column(TypeName = "nvarchar(450)")]
    public string? UploadedBy { get; set; }
    
    // Foreign keys (optional associations)
    public int? ArticleId { get; set; }
    public int? ProjectId { get; set; }
    
    // Navigation properties
    public virtual Article? Article { get; set; }
    public virtual Project? Project { get; set; }
    public virtual AspNetUser? UploadedByUser { get; set; }
}