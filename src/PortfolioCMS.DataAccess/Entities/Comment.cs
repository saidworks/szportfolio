using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PortfolioCMS.DataAccess.Entities;

[Table("Comments")]
[Index(nameof(Status))]
[Index(nameof(ArticleId))]
[Index(nameof(SubmittedDate))]
public class Comment
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string AuthorName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public string AuthorEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(2000)")]
    public string Content { get; set; } = string.Empty;
    
    [Column(TypeName = "datetime2")]
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public CommentStatus Status { get; set; } = CommentStatus.Pending;
    
    [MaxLength(45)]
    [Column(TypeName = "nvarchar(45)")]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string? UserAgent { get; set; }
    
    // Foreign key
    public int ArticleId { get; set; }
    
    // Optimistic concurrency control
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation property
    public virtual Article Article { get; set; } = null!;
}